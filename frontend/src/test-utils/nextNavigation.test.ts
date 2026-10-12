import { describe, expect, it } from 'vitest'

import {
  navigationState,
  notFound,
  notFoundError,
  permanentRedirect,
  redirect,
  resetNextNavigation,
  router,
  unstable_rethrow,
  useParams,
  usePathname,
  useRouter,
  useSearchParams,
} from './nextNavigation'

describe('shared Next.js navigation mock', () => {
  it('exposes the configured navigation state and stable router', () => {
    navigationState.pathname = '/portal'
    navigationState.searchParams = new URLSearchParams('page=2')
    navigationState.params = { id: '42', segments: ['users', 'edit'] }

    expect(useRouter()).toBe(router)
    expect(usePathname()).toBe('/portal')
    expect(useSearchParams().get('page')).toBe('2')
    expect(useParams()).toEqual({ id: '42', segments: ['users', 'edit'] })
  })

  it('resets navigation state, router calls and overridden implementations', () => {
    navigationState.pathname = '/portal'
    navigationState.searchParams.set('page', '2')
    navigationState.params.id = '42'
    for (const method of Object.values(router)) {
      method.mockImplementation(() => 'overridden')
      method()
    }
    redirect.mockImplementation(() => {
      throw new Error('overridden')
    })
    permanentRedirect.mockImplementation(() => {
      throw new Error('overridden')
    })
    notFound.mockImplementation(() => {
      throw new Error('overridden')
    })
    unstable_rethrow.mockImplementation(() => {
      throw new Error('overridden')
    })

    resetNextNavigation()

    expect(usePathname()).toBe('/')
    expect(useSearchParams().toString()).toBe('')
    expect(useParams()).toEqual({})
    for (const method of Object.values(router)) {
      expect(method).not.toHaveBeenCalled()
      expect(method()).toBeUndefined()
    }
    expect(redirect).not.toHaveBeenCalled()
    expect(permanentRedirect).not.toHaveBeenCalled()
    expect(notFound).not.toHaveBeenCalled()
    expect(unstable_rethrow).not.toHaveBeenCalled()
    expect(() => redirect('/sign-in')).toThrow('NEXT_REDIRECT: /sign-in')
    expect(() => permanentRedirect('/portal')).toThrow('NEXT_REDIRECT: /portal')
    expect(() => notFound()).toThrow(notFoundError)
    expect(() => unstable_rethrow(new Error('ordinary error'))).not.toThrow()
    expect(() => unstable_rethrow(notFoundError)).toThrow(notFoundError)
  })

  it.each([redirect, permanentRedirect])('rethrows redirect control-flow errors', (navigate) => {
    let navigationError: unknown
    try {
      navigate('/portal')
    } catch (error) {
      navigationError = error
    }

    expect(navigate).toHaveBeenCalledExactlyOnceWith('/portal')
    expect(() => unstable_rethrow(navigationError)).toThrow('NEXT_REDIRECT: /portal')
  })
})
