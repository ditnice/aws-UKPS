import { vi } from 'vitest'

// Shared `next/navigation` mock (see ADR-005). Register it in a test with:
//
//   vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
//
// Import `router`, `navigationState`, `redirect` or `notFound` to set state or assert calls.
// Global setup resets state, call history and implementations before each test.
// These mocks model control flow, not the full Next.js runtime.

export const router = {
  push: vi.fn(),
  replace: vi.fn(),
  back: vi.fn(),
  forward: vi.fn(),
  refresh: vi.fn(),
  prefetch: vi.fn(),
}

export const navigationState: {
  pathname: string
  searchParams: URLSearchParams
  params: Record<string, string | string[]>
} = {
  pathname: '/',
  searchParams: new URLSearchParams(),
  params: {},
}

export const useRouter = () => router
export const usePathname = () => navigationState.pathname
export const useSearchParams = () => navigationState.searchParams
export const useParams = () => navigationState.params

export const RedirectType = { push: 'push', replace: 'replace' } as const

class NavigationError extends Error {}

export const notFoundError = new NavigationError('NEXT_NOT_FOUND')

function throwRedirect(url: string, _type?: string): never {
  throw new NavigationError(`NEXT_REDIRECT: ${url}`)
}

function throwNotFound(): never {
  throw notFoundError
}

function rethrowNavigationError(error: unknown): void {
  if (error instanceof NavigationError) {
    throw error
  }
}

// Next.js terminates rendering rather than returning from these functions.
export const redirect = vi.fn(throwRedirect)
export const permanentRedirect = vi.fn(throwRedirect)
export const notFound = vi.fn(throwNotFound)
export const unstable_rethrow = vi.fn(rethrowNavigationError)

export function resetNextNavigation() {
  navigationState.pathname = '/'
  navigationState.searchParams = new URLSearchParams()
  navigationState.params = {}
  for (const method of Object.values(router)) {
    method.mockReset()
  }
  redirect.mockReset().mockImplementation(throwRedirect)
  permanentRedirect.mockReset().mockImplementation(throwRedirect)
  notFound.mockReset().mockImplementation(throwNotFound)
  unstable_rethrow.mockReset().mockImplementation(rethrowNavigationError)
}
