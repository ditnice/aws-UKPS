import { buildSignInHref, signInPath } from '@/lib/auth/routing'

import type { CreateClientConfig } from './generated/client.gen'

const browserBaseUrl = '/backend-api'
const refreshUrl = `${browserBaseUrl}/auth/refresh`

let browserRefreshPromise: Promise<boolean> | null = null
let browserRefreshGeneration = 0

export const createClientConfig: CreateClientConfig = (config) => {
  const isBrowser = typeof window !== 'undefined'

  return {
    ...config,
    baseUrl: isBrowser ? browserBaseUrl : process.env.BACKEND_API_BASE_URL,
    // TODO: Server Components cannot persist refreshed cookies. Until a renewal layer exists,
    // server-side 401 responses should redirect to authentication.
    fetch: isBrowser ? browserFetch : globalThis.fetch,
  }
}

const browserFetch: typeof fetch = async (input, init) => {
  const retryInput = getRetryInput(input, init)
  const refreshGeneration = browserRefreshGeneration
  const response = await globalThis.fetch(input, init)

  if (response.status !== 401 || isAuthRequest(input) || retryInput === null) {
    return response
  }

  const refreshAlreadyCompleted = refreshGeneration !== browserRefreshGeneration
  if (!refreshAlreadyCompleted && !(await refreshBrowserToken())) {
    redirectToSignIn()
    return response
  }

  const retryResponse = await globalThis.fetch(retryInput, init)
  if (retryResponse.status === 401) redirectToSignIn()

  return retryResponse
}

function refreshBrowserToken(): Promise<boolean> {
  if (!browserRefreshPromise) {
    browserRefreshPromise = performBrowserRefresh().finally(() => {
      browserRefreshPromise = null
    })
  }

  return browserRefreshPromise
}

async function performBrowserRefresh(): Promise<boolean> {
  try {
    const response = await globalThis.fetch(refreshUrl, {
      method: 'POST',
      credentials: 'include',
      headers: { 'X-CSRF-Token': getCookie('csrf_token') ?? '' },
    })

    if (response.ok) browserRefreshGeneration += 1
    return response.ok
  } catch {
    return false
  }
}

function getRetryInput(input: RequestInfo | URL, init?: RequestInit): RequestInfo | URL | null {
  if (init?.body && typeof ReadableStream !== 'undefined' && init.body instanceof ReadableStream) {
    return null
  }

  if (typeof Request !== 'undefined' && input instanceof Request) {
    try {
      return input.clone()
    } catch {
      return null
    }
  }

  return input
}

function isAuthRequest(input: RequestInfo | URL): boolean {
  const requestUrl =
    typeof Request !== 'undefined' && input instanceof Request ? input.url : input.toString()

  try {
    const baseUrl = typeof window !== 'undefined' ? window.location.origin : undefined
    const pathname = new URL(requestUrl, baseUrl).pathname

    return pathname === `${browserBaseUrl}/auth` || pathname.startsWith(`${browserBaseUrl}/auth/`)
  } catch {
    return false
  }
}

function redirectToSignIn(): void {
  if (typeof window === 'undefined') {
    return
  }

  const returnTo = `${globalThis.location.pathname}${globalThis.location.search}`
  if (globalThis.location.pathname === signInPath) {
    return
  }

  globalThis.location.replace(buildSignInHref(returnTo))
}

function getCookie(name: string): string | null {
  if (typeof document === 'undefined') {
    return null
  }

  for (const cookie of document.cookie.split(';')) {
    const separator = cookie.indexOf('=')
    const key = (separator === -1 ? cookie : cookie.slice(0, separator)).trim()

    if (key === name) {
      return decodeURIComponent(separator === -1 ? '' : cookie.slice(separator + 1))
    }
  }

  return null
}
