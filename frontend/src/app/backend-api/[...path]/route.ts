import type { NextRequest } from 'next/server'

export const runtime = 'nodejs'
export const dynamic = 'force-dynamic'

type RouteContext = {
  params: Promise<{ path: string[] }>
}

const requestHeaderAllowlist = [
  'accept',
  'accept-language',
  'baggage',
  'content-encoding',
  'content-language',
  'content-length',
  'content-type',
  'if-match',
  'if-modified-since',
  'if-none-match',
  'if-range',
  'if-unmodified-since',
  'origin',
  'range',
  'traceparent',
  'tracestate',
  'x-csrf-token',
  'x-request-id',
] as const

const responseHeaderAllowlist = [
  'accept-ranges',
  'content-disposition',
  'content-language',
  'content-range',
  'content-type',
  'etag',
  'last-modified',
  'retry-after',
  'traceparent',
  'tracestate',
  'vary',
  'www-authenticate',
  'x-request-id',
] as const

const allowedCookies = new Set(['access_token', 'refresh_token', 'csrf_token'])
const unsafeMethods = new Set(['POST', 'PUT', 'PATCH', 'DELETE'])
const defaultUpstreamTimeoutMs = 15_000
const privateNoStore = 'private, no-store'

async function proxyRequest(request: NextRequest, context: RouteContext): Promise<Response> {
  const backendApiBaseUrl = process.env.BACKEND_API_BASE_URL

  if (!backendApiBaseUrl) {
    return errorResponse(500, 'BACKEND_API_BASE_URL is not configured.')
  }

  if (unsafeMethods.has(request.method) && !hasSameOrigin(request)) {
    logOriginRejection(request)
    return errorResponse(403, 'Request origin is not allowed.')
  }

  const { path } = await context.params
  if (path.some((segment) => segment === '.' || segment === '..')) {
    return errorResponse(400, 'The requested backend path is invalid.')
  }

  let targetUrl: URL

  try {
    targetUrl = buildTargetUrl(backendApiBaseUrl, path, new URL(request.url).search)
  } catch {
    return errorResponse(500, 'BACKEND_API_BASE_URL is invalid.')
  }

  const requestHeaders = new Headers()
  for (const name of requestHeaderAllowlist) {
    const value = request.headers.get(name)
    if (value !== null) requestHeaders.set(name, value)
  }

  const cookie = filterRequestCookies(request.headers.get('cookie'))
  if (cookie) requestHeaders.set('cookie', cookie)

  const timeoutSignal = AbortSignal.timeout(getUpstreamTimeoutMs())
  const init: RequestInit = {
    cache: 'no-store',
    headers: requestHeaders,
    method: request.method,
    redirect: 'manual',
    signal: AbortSignal.any([request.signal, timeoutSignal]),
  }

  if (!['GET', 'HEAD'].includes(request.method)) {
    init.body = await request.arrayBuffer()
  }

  let backendResponse: Response
  try {
    backendResponse = await fetch(targetUrl, init)
  } catch {
    if (timeoutSignal.aborted) {
      return errorResponse(504, 'The upstream request timed out.')
    }

    return errorResponse(502, 'The upstream request failed.')
  }

  const responseHeaders = new Headers({ 'cache-control': privateNoStore })
  for (const name of responseHeaderAllowlist) {
    const value = backendResponse.headers.get(name)
    if (value !== null) responseHeaders.set(name, value)
  }

  const location = backendResponse.headers.get('location')
  if (location) {
    const rewrittenLocation = rewriteLocation(location, targetUrl, backendApiBaseUrl)
    if (rewrittenLocation) responseHeaders.set('location', rewrittenLocation)
  }

  for (const cookieHeader of getSetCookieHeaders(backendResponse.headers)) {
    const cookie = filterResponseCookie(cookieHeader)
    if (cookie) responseHeaders.append('set-cookie', cookie)
  }

  return new Response(backendResponse.body, {
    headers: responseHeaders,
    status: backendResponse.status,
    statusText: backendResponse.statusText,
  })
}

function buildTargetUrl(baseUrl: string, path: string[], search: string): URL {
  const targetUrl = new URL(baseUrl)
  if (
    !['http:', 'https:'].includes(targetUrl.protocol) ||
    targetUrl.username ||
    targetUrl.password
  ) {
    throw new TypeError('Invalid backend URL')
  }
  const basePath = targetUrl.pathname.endsWith('/') ? targetUrl.pathname : `${targetUrl.pathname}/`
  targetUrl.pathname = `${basePath}${path.map(encodeURIComponent).join('/')}`
  targetUrl.search = search
  targetUrl.hash = ''
  return targetUrl
}

function hasSameOrigin(request: NextRequest): boolean {
  const origin = request.headers.get('origin')
  if (!origin) return false

  try {
    const expectedOrigin = process.env.FRONTEND_PUBLIC_ORIGIN ?? new URL(request.url).origin
    return new URL(origin).origin === expectedOrigin
  } catch {
    return false
  }
}

function logOriginRejection(request: NextRequest): void {
  let requestOrigin = 'invalid'

  try {
    requestOrigin = new URL(request.url).origin
  } catch {
    // Keep diagnostics robust if Next supplies an unexpected URL.
  }

  console.warn('Backend API proxy rejected unsafe request origin.', {
    forwardedHost: request.headers.get('x-forwarded-host'),
    forwardedProto: request.headers.get('x-forwarded-proto'),
    host: request.headers.get('host'),
    method: request.method,
    origin: request.headers.get('origin'),
    requestOrigin,
    requestUrl: request.url,
  })
}

function getUpstreamTimeoutMs(): number {
  const configuredTimeout = Number(process.env.BACKEND_API_TIMEOUT_MS)
  return Number.isInteger(configuredTimeout) && configuredTimeout > 0
    ? configuredTimeout
    : defaultUpstreamTimeoutMs
}

function filterRequestCookies(cookieHeader: string | null): string {
  if (!cookieHeader) return ''

  return cookieHeader
    .split(';')
    .map((cookie) => cookie.trim())
    .filter((cookie) => allowedCookies.has(cookie.slice(0, cookie.indexOf('=')).trim()))
    .join('; ')
}

function filterResponseCookie(cookieHeader: string): string | null {
  const parts = cookieHeader.split(';')
  const separator = parts[0].indexOf('=')
  const name = separator === -1 ? '' : parts[0].slice(0, separator).trim()

  if (!allowedCookies.has(name) || parts.slice(1).some((part) => /^\s*domain\s*=/i.test(part))) {
    return null
  }

  if (name === 'refresh_token') {
    let hasExpectedPath = false

    for (let index = 1; index < parts.length; index += 1) {
      if (/^\s*path\s*=\s*\/backend-api\/auth\/refresh\s*$/i.test(parts[index])) {
        hasExpectedPath = true
      }

      if (/^\s*path\s*=\s*\/auth\/refresh\s*$/i.test(parts[index])) {
        hasExpectedPath = true
      }

      parts[index] = parts[index].replace(
        /^(\s*path\s*=\s*)\/auth\/refresh(\s*)$/i,
        '$1/backend-api/auth/refresh$2',
      )
    }

    if (!hasExpectedPath) return null
  }

  return parts.join(';')
}

function rewriteLocation(location: string, targetUrl: URL, baseUrl: string): string | null {
  let resolvedLocation: URL
  let backendBaseUrl: URL

  try {
    resolvedLocation = new URL(location, targetUrl)
    backendBaseUrl = new URL(baseUrl)
  } catch {
    return null
  }

  if (resolvedLocation.origin !== backendBaseUrl.origin) return null

  const basePath = backendBaseUrl.pathname.replace(/\/$/, '')
  if (
    basePath &&
    resolvedLocation.pathname !== basePath &&
    !resolvedLocation.pathname.startsWith(`${basePath}/`)
  ) {
    return null
  }

  const backendPath = resolvedLocation.pathname.slice(basePath.length) || '/'
  return `/backend-api${backendPath}${resolvedLocation.search}${resolvedLocation.hash}`
}

function getSetCookieHeaders(headers: Headers): string[] {
  const headersWithGetSetCookie = headers as Headers & { getSetCookie?: () => string[] }
  return headersWithGetSetCookie.getSetCookie?.() ?? []
}

function errorResponse(status: number, error: string): Response {
  return Response.json({ error }, { headers: { 'cache-control': privateNoStore }, status })
}

export const GET = proxyRequest
export const POST = proxyRequest
export const PUT = proxyRequest
export const PATCH = proxyRequest
export const DELETE = proxyRequest
export const HEAD = proxyRequest
export const OPTIONS = proxyRequest
