import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { DELETE, dynamic, GET, POST, runtime } from './route'

const originalBaseUrl = process.env.BACKEND_API_BASE_URL

function request(method = 'GET', headers: HeadersInit = {}, body?: string) {
  return new Request('https://frontend.example/backend-api/users?active=true', {
    body,
    headers,
    method,
  })
}

function context(...path: string[]) {
  return { params: Promise.resolve({ path }) }
}

function responseWithCookies(cookies: string[], headers: HeadersInit = {}) {
  const responseHeaders = new Headers(headers) as Headers & { getSetCookie: () => string[] }
  responseHeaders.getSetCookie = () => cookies
  return {
    body: new Response('upstream').body,
    headers: responseHeaders,
    status: 200,
    statusText: 'OK',
  } as Response
}

beforeEach(() => {
  process.env.BACKEND_API_BASE_URL = 'https://api.example/v1'
  vi.stubGlobal(
    'fetch',
    vi.fn(async () => new Response('ok')),
  )
})

afterEach(() => {
  vi.unstubAllEnvs()
  vi.unstubAllGlobals()
  vi.restoreAllMocks()
  if (originalBaseUrl === undefined) delete process.env.BACKEND_API_BASE_URL
  else process.env.BACKEND_API_BASE_URL = originalBaseUrl
})

describe('backend API route', () => {
  it('uses the Node runtime and forces dynamic handling', () => {
    expect(runtime).toBe('nodejs')
    expect(dynamic).toBe('force-dynamic')
  })

  it('constructs a URL below the configured base path and preserves only the request query', async () => {
    process.env.BACKEND_API_BASE_URL = 'https://api.example/root/api?configured=ignored#fragment'

    await GET(request() as never, context('people', 'a/b'))

    const [url, init] = vi.mocked(fetch).mock.calls[0]
    expect(String(url)).toBe('https://api.example/root/api/people/a%2Fb?active=true')
    expect(init).toMatchObject({ cache: 'no-store', method: 'GET', redirect: 'manual' })
  })

  it('returns a private error for missing or invalid configuration', async () => {
    delete process.env.BACKEND_API_BASE_URL
    const missing = await GET(request() as never, context('users'))
    process.env.BACKEND_API_BASE_URL = 'file:///etc/passwd'
    const invalid = await GET(request() as never, context('users'))

    expect(missing.status).toBe(500)
    expect(invalid.status).toBe(500)
    expect(missing.headers.get('cache-control')).toBe('private, no-store')
    expect(fetch).not.toHaveBeenCalled()
  })

  it('allows safe methods without Origin and requires same-origin Origin for unsafe methods', async () => {
    const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => undefined)

    expect((await GET(request() as never, context('users'))).status).toBe(200)
    expect((await POST(request('POST') as never, context('users'))).status).toBe(403)
    expect(
      (
        await DELETE(
          request('DELETE', { origin: 'https://attacker.example' }) as never,
          context('users'),
        )
      ).status,
    ).toBe(403)
    expect(
      (
        await POST(
          request('POST', { origin: 'https://frontend.example' }) as never,
          context('users'),
        )
      ).status,
    ).toBe(200)
    expect(consoleWarn).toHaveBeenCalledWith(
      'Backend API proxy rejected unsafe request origin.',
      expect.objectContaining({
        method: 'POST',
        origin: null,
        requestOrigin: 'https://frontend.example',
        requestUrl: 'https://frontend.example/backend-api/users?active=true',
      }),
    )
  })

  it('forwards only allowlisted request headers and cookies', async () => {
    await POST(
      request(
        'POST',
        {
          authorization: 'Bearer browser-secret',
          baggage: 'tenant=one',
          cookie: 'access_token=a; other=secret; csrf_token=c; refresh_token=r',
          'if-match': '"version"',
          origin: 'https://frontend.example',
          range: 'bytes=0-9',
          traceparent: '00-trace-parent-01',
          'x-csrf-token': 'csrf',
          'x-untrusted': 'no',
        },
        '{}',
      ) as never,
      context('users'),
    )

    const headers = new Headers(vi.mocked(fetch).mock.calls[0][1]?.headers)
    expect(headers.get('cookie')).toBe('access_token=a; csrf_token=c; refresh_token=r')
    expect(headers.get('x-csrf-token')).toBe('csrf')
    expect(headers.get('traceparent')).toBe('00-trace-parent-01')
    expect(headers.get('baggage')).toBe('tenant=one')
    expect(headers.get('if-match')).toBe('"version"')
    expect(headers.get('range')).toBe('bytes=0-9')
    expect(headers.has('authorization')).toBe(false)
    expect(headers.get('origin')).toBe('https://frontend.example')
    expect(headers.has('x-untrusted')).toBe(false)
  })

  it('preserves selected response headers and always prevents response caching', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response('denied', {
        headers: {
          'cache-control': 'public, max-age=3600',
          'content-encoding': 'gzip',
          'content-length': '10',
          'content-type': 'application/problem+json',
          etag: '"one"',
          server: 'secret',
          'www-authenticate': 'Bearer realm="api"',
          'x-request-id': 'request-1',
        },
        status: 401,
      }),
    )

    const response = await GET(request() as never, context('users'))

    expect(response.status).toBe(401)
    expect(response.headers.get('cache-control')).toBe('private, no-store')
    expect(response.headers.get('content-type')).toBe('application/problem+json')
    expect(response.headers.has('content-encoding')).toBe(false)
    expect(response.headers.has('content-length')).toBe(false)
    expect(response.headers.get('etag')).toBe('"one"')
    expect(response.headers.get('www-authenticate')).toBe('Bearer realm="api"')
    expect(response.headers.get('x-request-id')).toBe('request-1')
    expect(response.headers.has('server')).toBe(false)
  })

  it('rewrites same-backend redirects and drops external or out-of-base redirects', async () => {
    vi.mocked(fetch)
      .mockResolvedValueOnce(
        new Response(null, {
          headers: { location: 'https://api.example/v1/auth/login?return=true' },
          status: 307,
        }),
      )
      .mockResolvedValueOnce(
        new Response(null, {
          headers: { location: 'https://attacker.example/login' },
          status: 307,
        }),
      )
      .mockResolvedValueOnce(
        new Response(null, {
          headers: { location: 'https://api.example/admin' },
          status: 307,
        }),
      )

    const internal = await GET(request() as never, context('users'))
    const external = await GET(request() as never, context('users'))
    const outsideBase = await GET(request() as never, context('users'))

    expect(internal.headers.get('location')).toBe('/backend-api/auth/login?return=true')
    expect(external.headers.has('location')).toBe(false)
    expect(outsideBase.headers.has('location')).toBe(false)
  })

  it('passes through only known host-only cookies and rewrites only the refresh path', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      responseWithCookies([
        'access_token=a; Path=/; HttpOnly; Secure; SameSite=Lax',
        'csrf_token=c; Path=/; Secure; SameSite=Strict',
        'refresh_token=r; Path=/auth/refresh; Max-Age=3600; HttpOnly; Secure',
        'refresh_token=; Path=/auth/refresh; Max-Age=0; HttpOnly',
        'refresh_token=wrong; Path=/other; HttpOnly',
        'access_token=bad; Domain=example.com; Path=/; HttpOnly',
        'unknown=value; Path=/; HttpOnly',
      ]),
    )
    const appendSpy = vi.spyOn(Headers.prototype, 'append')

    await GET(request() as never, context('users'))
    const cookies = appendSpy.mock.calls
      .filter(([name]) => name.toLowerCase() === 'set-cookie')
      .map(([, value]) => value)

    expect(cookies).toContain('access_token=a; Path=/; HttpOnly; Secure; SameSite=Lax')
    expect(cookies).toContain('csrf_token=c; Path=/; Secure; SameSite=Strict')
    expect(cookies).toContain(
      'refresh_token=r; Path=/backend-api/auth/refresh; Max-Age=3600; HttpOnly; Secure',
    )
    expect(cookies).toContain('refresh_token=; Path=/backend-api/auth/refresh; Max-Age=0; HttpOnly')
    expect(cookies).not.toContain('refresh_token=wrong; Path=/other; HttpOnly')
    expect(cookies).not.toContain('Domain=')
    expect(cookies).not.toContain('unknown=')
  })

  it('returns controlled gateway responses for upstream failure and timeout', async () => {
    vi.mocked(fetch).mockRejectedValueOnce(new TypeError('connection refused'))
    const failed = await GET(request() as never, context('users'))

    const timeoutController = new AbortController()
    const timeoutSpy = vi
      .spyOn(AbortSignal, 'timeout')
      .mockReturnValueOnce(timeoutController.signal)
    vi.mocked(fetch).mockImplementationOnce(
      (_url, init) =>
        new Promise((_resolve, reject) => {
          init?.signal?.addEventListener('abort', () => reject(init.signal?.reason))
        }),
    )
    const timeoutPromise = GET(request() as never, context('users'))
    await vi.waitFor(() => expect(fetch).toHaveBeenCalledTimes(2))
    timeoutController.abort(new DOMException('Timed out', 'TimeoutError'))
    const timedOut = await timeoutPromise

    expect(failed.status).toBe(502)
    expect(await failed.json()).toEqual({ error: 'The upstream request failed.' })
    expect(timeoutSpy).toHaveBeenCalledWith(15_000)
    expect(timedOut.status).toBe(504)
    expect(await timedOut.json()).toEqual({ error: 'The upstream request timed out.' })
  })

  it('uses a configured positive upstream timeout', async () => {
    vi.stubEnv('BACKEND_API_TIMEOUT_MS', '2500')
    const timeoutSpy = vi.spyOn(AbortSignal, 'timeout')

    await GET(request() as never, context('users'))

    expect(timeoutSpy).toHaveBeenCalledWith(2500)
  })

  it('passes client cancellation to fetch and does not buffer the upstream response', async () => {
    const controller = new AbortController()
    const incoming = new Request('https://frontend.example/backend-api/download', {
      signal: controller.signal,
    })
    const stream = new ReadableStream({ start() {} })
    vi.mocked(fetch).mockResolvedValueOnce(new Response(stream))

    const response = await GET(incoming as never, context('download'))
    const upstreamSignal = vi.mocked(fetch).mock.calls[0][1]?.signal
    controller.abort()

    expect(upstreamSignal?.aborted).toBe(true)
    expect(response.body).not.toBeNull()
  })
})
