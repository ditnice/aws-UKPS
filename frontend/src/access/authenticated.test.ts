import { describe, expect, it } from 'vitest'

import { accessArgs, payloadUser } from '@/test-utils/payloadAccess'

import { authenticated } from './authenticated'

describe('authenticated', () => {
  it('allows access when the request has an authenticated user', () => {
    expect(authenticated(accessArgs(payloadUser))).toBe(true)
  })

  it.each([null, undefined])('denies access when the request user is %s', (user) => {
    expect(authenticated(accessArgs(user))).toBe(false)
  })
})
