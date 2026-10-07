import { afterEach, describe, expect, it, vi } from 'vitest'

import { getCookie } from './cookies'

afterEach(() => {
  vi.unstubAllGlobals()
  vi.restoreAllMocks()

  document.cookie = 'csrf_token=; Max-Age=0; path=/'
  document.cookie = 'other=; Max-Age=0; path=/'
})

describe('getCookie', () => {
  it('returns the decoded value of the named cookie', () => {
    document.cookie = 'other=value; path=/'
    document.cookie = 'csrf_token=a%20b; path=/'

    expect(getCookie('csrf_token')).toBe('a b')
  })

  it('returns null when the cookie is not set', () => {
    expect(getCookie('csrf_token')).toBeNull()
  })

  it('returns null outside of a browser environment', () => {
    vi.stubGlobal('document', undefined)

    expect(getCookie('csrf_token')).toBeNull()
  })

  it('returns an empty stringfor a matching cookie without a separator', () => {
    vi.spyOn(document, 'cookie', 'get').mockReturnValue('csrf_token')

    expect(getCookie('csrf_token')).toBe('')
  })
})
