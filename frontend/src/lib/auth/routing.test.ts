import { describe, expect, it } from 'vitest'

import { buildSignInHref, getSafeReturnTo, signInPath } from './routing'

describe('auth routing', () => {
  it('returns safe relative returnTo paths', () => {
    expect(getSafeReturnTo('/portal/organisations/1?tab=users')).toBe(
      '/portal/organisations/1?tab=users',
    )
  })

  it('rejects unsafe returnTo values', () => {
    expect(getSafeReturnTo('https://example.com/portal')).toBeUndefined()
    expect(getSafeReturnTo('//example.com/portal')).toBeUndefined()
    expect(getSafeReturnTo('')).toBeUndefined()
  })

  it('builds a sign-in href with an encoded returnTo', () => {
    expect(buildSignInHref('/portal/organisations/1?tab=users')).toBe(
      '/auth/sign-in?returnTo=%2Fportal%2Forganisations%2F1%3Ftab%3Dusers',
    )
  })

  it('omits unsafe returnTo values from sign-in hrefs', () => {
    expect(buildSignInHref('https://example.com/portal')).toBe(signInPath)
  })
})
