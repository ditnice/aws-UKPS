import { describe, expect, it } from 'vitest'

import { buildUserActionHref, parseUserAction } from './userActionAlert'

describe('parseUserAction', () => {
  it('reads an invited user id', () => {
    expect(parseUserAction({ action: 'invited', userId: '456' })).toEqual({
      action: 'invited',
      userId: 456,
    })
  })

  it('reads a permissions updated user id', () => {
    expect(parseUserAction({ action: 'permissions-updated', userId: '4' })).toEqual({
      action: 'permissions-updated',
      userId: 4,
    })
  })

  it('ignores a missing action', () => {
    expect(parseUserAction({ userId: '4' })).toBeUndefined()
  })

  it('ignores an action it does not recognise', () => {
    expect(parseUserAction({ action: 'deactivated', userId: '4' })).toBeUndefined()
  })

  it.each(['', ' ', 'test@test.com', '0', '-3', '1.5'])(
    'ignores %j, which is not a user id',
    (userId) => {
      expect(parseUserAction({ action: 'invited', userId })).toBeUndefined()
    },
  )
})

describe('buildUserActionHref', () => {
  it('links back to the organisation page after an invite', () => {
    expect(buildUserActionHref(123, 'invited', 456)).toBe(
      '/portal/organisations/123?action=invited&userId=456',
    )
  })

  it('links back to the organisation page after a permissions change', () => {
    expect(buildUserActionHref(2, 'permissions-updated', 4)).toBe(
      '/portal/organisations/2?action=permissions-updated&userId=4',
    )
  })
})
