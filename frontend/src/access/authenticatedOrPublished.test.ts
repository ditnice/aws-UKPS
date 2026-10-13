import { describe, expect, it } from 'vitest'

import { accessArgs, payloadUser } from '@/test-utils/payloadAccess'

import { authenticatedOrPublished } from './authenticatedOrPublished'

describe('authenticatedOrPublished', () => {
  it('allows an authenticated user to access content without a publication constraint', () => {
    expect(authenticatedOrPublished(accessArgs(payloadUser))).toBe(true)
  })

  it.each([null, undefined])(
    'restricts content to published documents when the request user is %s',
    (user) => {
      expect(authenticatedOrPublished(accessArgs(user))).toEqual({
        _status: { equals: 'published' },
      })
    },
  )
})
