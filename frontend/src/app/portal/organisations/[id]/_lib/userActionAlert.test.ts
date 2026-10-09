import { describe, expect, it } from 'vitest'

import { buildUserActionHref, parseUserAction } from './userActionAlert'

describe('parseUserAction', () => {
  const requestGuid = 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d'

  it.each(['approved-request', 'rejected-request'])('reads a GUID for %s', (action) => {
    expect(parseUserAction({ action, userRequestId: requestGuid })).toEqual({
      type: 'request',
      action,
      userRequestId: requestGuid,
    })
  })

  it.each([undefined, '', '4', 'not-a-guid', `${requestGuid}extra`])(
    'ignores an invalid request GUID %j',
    (userRequestId) => {
      expect(parseUserAction({ action: 'approved-request', userRequestId })).toBeUndefined()
    },
  )

  it('reads an invited user id', () => {
    expect(parseUserAction({ action: 'invited', userId: '456' })).toEqual({
      type: 'user',
      action: 'invited',
      userId: 456,
    })
  })

  it('reads a permissions updated user id', () => {
    expect(parseUserAction({ action: 'permissions-updated', userId: '4' })).toEqual({
      type: 'user',
      action: 'permissions-updated',
      userId: 4,
    })
  })

  it('reads a deactivated user id', () => {
    expect(parseUserAction({ action: 'deactivated', userId: '4' })).toEqual({
      type: 'user',
      action: 'deactivated',
      userId: 4,
    })
  })

  it('reads a reactivated user id', () => {
    expect(parseUserAction({ action: 'reactivated', userId: '4' })).toEqual({
      type: 'user',
      action: 'reactivated',
      userId: 4,
    })
  })

  it.each(['approved-request', 'rejected-request'] as const)(
    'rejects a legacy numeric request id for %s',
    (action) => {
      expect(parseUserAction({ action, userRequestId: '4' })).toBeUndefined()
    },
  )

  it.each(['approved-request', 'rejected-request'] as const)(
    'ignores a %s action with an invalid request id',
    (action) => {
      for (const userRequestId of [undefined, '', ' ', 'invalid', '0', '-3', '1.5']) {
        expect(parseUserAction({ action, userRequestId })).toBeUndefined()
      }
    },
  )

  it.each(['approved-request', 'rejected-request'] as const)(
    'ignores a %s action with only a user id',
    (action) => {
      expect(parseUserAction({ action, userId: '4' })).toBeUndefined()
    },
  )

  it('ignores an action it does not recognise', () => {
    expect(parseUserAction({ action: 'never-going-to-be-an-action', userId: '4' })).toBeUndefined()
  })

  it('ignores a missing action', () => {
    expect(parseUserAction({ userId: '4' })).toBeUndefined()
  })

  it.each(['', ' ', 'test@test.com', '0', '-3', '1.5'])(
    'ignores %j, which is not a user id',
    (userId) => {
      expect(parseUserAction({ action: 'invited', userId })).toBeUndefined()
    },
  )
})

describe('buildUserActionHref', () => {
  it.each(['approved-request', 'rejected-request'] as const)(
    'preserves the request GUID after %s',
    (action) => {
      const requestGuid = 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d'
      expect(buildUserActionHref(123, { action, userRequestId: requestGuid })).toBe(
        `/portal/organisations/123?action=${action}&userRequestId=${requestGuid}`,
      )
    },
  )

  it('links back to the organisation page after an invite', () => {
    expect(buildUserActionHref(123, { action: 'invited', userId: 456 })).toBe(
      '/portal/organisations/123?action=invited&userId=456',
    )
  })

  it('links back to the organisation page after a permissions change', () => {
    expect(buildUserActionHref(2, { action: 'permissions-updated', userId: 4 })).toBe(
      '/portal/organisations/2?action=permissions-updated&userId=4',
    )
  })

  it.each(['approved-request', 'rejected-request'] as const)(
    'links back to the organisation page after a %s',
    (action) => {
      const requestGuid = 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d'
      expect(buildUserActionHref(2, { action, userRequestId: requestGuid })).toBe(
        `/portal/organisations/2?action=${action}&userRequestId=${requestGuid}`,
      )
    },
  )
})
