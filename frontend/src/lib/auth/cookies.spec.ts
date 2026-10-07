import { afterEach, describe, expect, it } from 'vitest'

import { getCookie } from './cookies'

afterEach(() => {
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
})
