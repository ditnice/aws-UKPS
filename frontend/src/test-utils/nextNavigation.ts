import { vi } from 'vitest'

// Shared `next/navigation` mock (see ADR-005). Register it in a test with:
//
//   vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
//
// and import `router`, `navigationState`, `redirect` or `notFound` from here to set state or
// assert calls. `vi.clearAllMocks()` runs after every test, so call history doesn't leak between
// tests; `navigationState` is plain data, so set it in `beforeEach` when a test depends on it.

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

// Next.js stops rendering by throwing from these, so the mocks throw too.
export const redirect = vi.fn((url: string, _type?: string): never => {
  throw new Error(`NEXT_REDIRECT: ${url}`)
})
export const permanentRedirect = vi.fn((url: string, _type?: string): never => {
  throw new Error(`NEXT_REDIRECT: ${url}`)
})
export const notFound = vi.fn((): never => {
  throw new Error('NEXT_NOT_FOUND')
})
