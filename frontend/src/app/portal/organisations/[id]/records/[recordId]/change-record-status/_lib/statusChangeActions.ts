import { archiveRecord, putRecordOnHold, resumeRecord } from '@/client/generated'
import type {
  RecordDto,
  RecordStatus,
  RecordStatusChangeReason,
  RecordType,
} from '@/client/generated'
import { getOrganisationSelectionHref } from '@/lib/auth/routing'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { buildRecordHref } from '../../_lib/recordLinks'

import type { RecordViewAlertAction } from '../../_components/RecordViewAlert'

export type RecordStatusChangeAction = 'Archive' | 'PlaceOnHold' | 'RemoveHold'

type RecordStatusChangeDefinition = {
  label: string
  path: string
  fromStatuses: readonly RecordStatus[]
  /** The alert shown on the record page once the change is made. */
  changedAlert: RecordViewAlertAction
}

/** The status changes a user can make, in the order they are offered. */
export const statusChangeActions: Record<RecordStatusChangeAction, RecordStatusChangeDefinition> = {
  PlaceOnHold: {
    label: 'Place the record on hold',
    path: 'place-on-hold',
    fromStatuses: ['Published'],
    changedAlert: 'placed-on-hold',
  },
  RemoveHold: {
    label: 'Remove hold and return to published',
    path: 'remove-hold',
    fromStatuses: ['OnHold'],
    changedAlert: 'hold-removed',
  },
  Archive: {
    label: 'Archive the record',
    path: 'archive',
    fromStatuses: ['Published', 'OnHold'],
    changedAlert: 'archived',
  },
}

export const statusChangeActionOrder = Object.keys(
  statusChangeActions,
) as RecordStatusChangeAction[]

export type StatusChangeStep = 'reason' | 'confirm'

export const buildStatusChangeHref = (
  organisationId: number,
  recordId: number,
  recordType: RecordType,
  action: RecordStatusChangeAction,
  step: StatusChangeStep,
) =>
  buildRecordHref(
    organisationId,
    recordId,
    recordType,
    `change-record-status/${statusChangeActions[action].path}${step === 'confirm' ? '/confirm' : ''}`,
  )

export const isStatusChangeAvailable = (action: RecordStatusChangeAction, status: RecordStatus) =>
  statusChangeActions[action].fromStatuses.includes(status)

export const canMakeStatusChange = (
  record: Pick<RecordDto, 'organisationId' | 'actions' | 'recordStatus'>,
  organisationId: number,
  action: RecordStatusChangeAction,
) =>
  record.organisationId === organisationId &&
  record.actions.includes('ChangeStatus') &&
  isStatusChangeAvailable(action, record.recordStatus)

type StatusChangeRequest = {
  organisationId: number
  recordId: number
  reason: RecordStatusChangeReason
  note?: string
}

export const submitStatusChange = (
  action: RecordStatusChangeAction,
  { organisationId, recordId, reason, note }: StatusChangeRequest,
) => {
  const options = { path: { recordId }, query: { organisationId }, body: { reason, note } }
  switch (action) {
    case 'Archive':
      return archiveRecord(options)
    case 'PlaceOnHold':
      return putRecordOnHold(options)
    case 'RemoveHold':
      return resumeRecord(options)
  }
}

export const statusChangeErrorMessages = {
  invalidReason: 'The reason you selected cannot be used for this change. Select another reason.',
  noteRequired: 'Enter additional information',
  notPermitted:
    'The status of this record has changed, so it cannot be changed in this way. Return to the record to see its current status.',
  recordChanged:
    'This record was updated by someone else while you were changing its status. Please try again.',
  failed: 'There was a problem changing the status of this record. Please try again later.',
} as const

/**
 * The message to show when a status change fails, or undefined when the client is already sending
 * the user to select or switch organisation.
 */
export const getStatusChangeErrorMessage = (
  status: number | undefined,
  error: unknown,
): string | undefined => {
  const problem = error && typeof error === 'object' ? (error as { code?: unknown }) : null

  if (getOrganisationSelectionHref(problem, undefined)) {
    return undefined
  }

  if (status === 400) {
    return isValidationProblemDetails(error) && 'Note' in error.errors
      ? statusChangeErrorMessages.noteRequired
      : statusChangeErrorMessages.invalidReason
  }

  if (status === 409 && problem?.code === 'status_change_not_permitted') {
    return statusChangeErrorMessages.notPermitted
  }

  if (status === 409 && problem?.code === 'record_changed') {
    return statusChangeErrorMessages.recordChanged
  }

  return statusChangeErrorMessages.failed
}
