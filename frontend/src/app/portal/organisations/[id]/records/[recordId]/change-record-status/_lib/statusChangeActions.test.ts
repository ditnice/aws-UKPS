import { beforeEach, describe, expect, it, vi } from 'vitest'

import { archiveRecord, putRecordOnHold, resumeRecord } from '@/client/generated'
import type { RecordAction, RecordStatus } from '@/client/generated'

import {
  buildStatusChangeHref,
  canMakeStatusChange,
  getStatusChangeErrorMessage,
  isStatusChangeAvailable,
  statusChangeErrorMessages,
  submitStatusChange,
  type RecordStatusChangeAction,
} from './statusChangeActions'

vi.mock('@/client/generated', () => ({
  archiveRecord: vi.fn(),
  putRecordOnHold: vi.fn(),
  resumeRecord: vi.fn(),
}))

beforeEach(() => {
  vi.clearAllMocks()
})

describe('buildStatusChangeHref', () => {
  it.each<[RecordStatusChangeAction, 'reason' | 'confirm', string]>([
    ['Archive', 'reason', 'archive'],
    ['Archive', 'confirm', 'archive/confirm'],
    ['PlaceOnHold', 'reason', 'place-on-hold'],
    ['PlaceOnHold', 'confirm', 'place-on-hold/confirm'],
    ['RemoveHold', 'reason', 'remove-hold'],
    ['RemoveHold', 'confirm', 'remove-hold/confirm'],
  ])('links %s %s to its page', (action, step, path) => {
    expect(buildStatusChangeHref(7, 42, 'Medicine', action, step)).toBe(
      `/portal/organisations/7/records/42/change-record-status/${path}?recordType=Medicine`,
    )
  })
})

describe('isStatusChangeAvailable', () => {
  it.each<[RecordStatusChangeAction, RecordStatus, boolean]>([
    ['Archive', 'Published', true],
    ['Archive', 'OnHold', true],
    ['Archive', 'Unpublished', false],
    ['Archive', 'Archived', false],
    ['PlaceOnHold', 'Published', true],
    ['PlaceOnHold', 'OnHold', false],
    ['PlaceOnHold', 'Unpublished', false],
    ['PlaceOnHold', 'Archived', false],
    ['RemoveHold', 'OnHold', true],
    ['RemoveHold', 'Published', false],
    ['RemoveHold', 'Unpublished', false],
    ['RemoveHold', 'Archived', false],
  ])('%s from %s is %s', (action, status, expected) => {
    expect(isStatusChangeAvailable(action, status)).toBe(expected)
  })
})

describe('canMakeStatusChange', () => {
  const record = {
    organisationId: 7,
    actions: ['Update', 'ChangeStatus', 'ViewTimeline'] as RecordAction[],
    recordStatus: 'Published' as RecordStatus,
  }

  it('allows a change the record status permits when the user can change status', () => {
    expect(canMakeStatusChange(record, 7, 'PlaceOnHold')).toBe(true)
  })

  it('rejects a record in another organisation', () => {
    expect(canMakeStatusChange(record, 8, 'PlaceOnHold')).toBe(false)
  })

  it('rejects a user who cannot change the status', () => {
    expect(canMakeStatusChange({ ...record, actions: ['ViewTimeline'] }, 7, 'PlaceOnHold')).toBe(
      false,
    )
  })

  it('rejects a change the record status does not permit', () => {
    expect(canMakeStatusChange(record, 7, 'RemoveHold')).toBe(false)
  })
})

describe('submitStatusChange', () => {
  it.each<[RecordStatusChangeAction, typeof archiveRecord]>([
    ['Archive', archiveRecord],
    ['PlaceOnHold', putRecordOnHold],
    ['RemoveHold', resumeRecord],
  ])('sends %s to its endpoint with the reason', async (action, endpoint) => {
    vi.mocked(endpoint).mockResolvedValue({} as Awaited<ReturnType<typeof endpoint>>)

    await submitStatusChange(action, {
      organisationId: 7,
      recordId: 42,
      reason: 'Other',
      note: 'Detail',
    })

    expect(endpoint).toHaveBeenCalledExactlyOnceWith({
      path: { recordId: 42 },
      query: { organisationId: 7 },
      body: { reason: 'Other', note: 'Detail' },
    })
    for (const other of [archiveRecord, putRecordOnHold, resumeRecord].filter(
      (x) => x !== endpoint,
    )) {
      expect(other).not.toHaveBeenCalled()
    }
  })
})

describe('getStatusChangeErrorMessage', () => {
  it.each([
    ['an invalid reason', 400, { errors: { Reason: ['Invalid'] } }, 'invalidReason'],
    ['a missing note', 400, { errors: { Note: ['Required'] } }, 'noteRequired'],
    [
      'a change the record no longer permits',
      409,
      { code: 'status_change_not_permitted' },
      'notPermitted',
    ],
    ['a concurrent update', 409, { code: 'record_changed' }, 'recordChanged'],
    ['an unknown conflict', 409, { code: 'something_else' }, 'failed'],
    ['a forbidden response', 403, {}, 'failed'],
    ['a not found response', 404, {}, 'failed'],
    ['a server error', 500, {}, 'failed'],
    ['a network failure', undefined, undefined, 'failed'],
  ] as const)('describes %s', (_, status, error, expected) => {
    expect(getStatusChangeErrorMessage(status, error)).toBe(statusChangeErrorMessages[expected])
  })

  it.each([
    ['a mismatched organisation', 409, { code: 'SelectedOrganisationMismatch', organisationId: 2 }],
    ['no selected organisation', 401, { code: 'SelectedOrganisationRequired' }],
  ])('leaves %s to the central organisation handling', (_, status, error) => {
    expect(getStatusChangeErrorMessage(status, error)).toBeUndefined()
  })
})
