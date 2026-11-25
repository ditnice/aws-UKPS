import { csrfCookieName, csrfHeaderName, getCookie } from '@/lib/auth/cookies'
import {
  buildSignInHref,
  getOrganisationSelectionHref,
  selectOrganisationPath,
  signInPath,
  switchOrganisationPath,
} from '@/lib/auth/routing'

import type { CreateClientConfig } from './generated/client.gen'

const browserBaseUrl = '/backend-api'
const refreshUrl = `${browserBaseUrl}/auth/refresh`

let browserRefreshPromise: Promise<boolean> | null = null
let browserRefreshGeneration = 0

export const createClientConfig: CreateClientConfig = (config) => {
  const isBrowser = typeof window !== 'undefined'

  return {
    ...config,
    baseUrl: isBrowser ? browserBaseUrl : undefined,
    fetch: isBrowser ? browserFetch : rejectServerFetch,
    // The client otherwise returns errors, which callers could mistake for a failed response.
    ...(!isBrowser && { throwOnError: true }),
  }
}

const rejectServerFetch: typeof fetch = async () => {
  throw new Error(
    'The default API client cannot be used on the server. Pass a client from createServerApiClient or createPublicServerApiClient.',
  )
}

const browserFetch: typeof fetch = async (input, init) => {
  const retryInput = getRetryInput(input, init)
  const refreshGeneration = browserRefreshGeneration
  const response = await globalThis.fetch(input, init)

  if (isAuthRequest(input)) {
    return response
  }

  // The session is valid but needs an organisation selected, so refreshing would not help.
  if (await redirectToOrganisationSelection(response)) {
    return response
  }

  if (response.status !== 401 || retryInput === null) {
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
      headers: { [csrfHeaderName]: getCookie(csrfCookieName) ?? '' },
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

async function redirectToOrganisationSelection(response: Response): Promise<boolean> {
  if (response.status !== 401 && response.status !== 409) {
    return false
  }

  const content = await response
    .clone()
    .json()
    .catch(() => null)
  const { pathname, search } = globalThis.location
  const href = getOrganisationSelectionHref(content, `${pathname}${search}`)

  if (!href) {
    return false
  }

  if (pathname !== selectOrganisationPath && pathname !== switchOrganisationPath) {
    globalThis.location.replace(href)
  }
  return true
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
