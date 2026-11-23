import { useSyncExternalStore } from 'react'

import { RecordStatusChangeReason } from '@/client/generated/types.gen'

import type { RecordStatusChangeAction } from './statusChangeActions'

/** The answers given on the reason page, kept for the confirmation page. */
export type ReasonAnswers = {
  reason: RecordStatusChangeReason
  note?: string
}

const storageKey = (recordId: number, action: RecordStatusChangeAction) =>
  `record-status-change:${recordId}:${action}`

const isReason = (value: unknown): value is RecordStatusChangeReason =>
  Object.values<unknown>(RecordStatusChangeReason).includes(value)

/** Saves the answers for this tab only. Returns false when the browser blocks storage. */
export function saveReasonAnswers(
  recordId: number,
  action: RecordStatusChangeAction,
  answers: ReasonAnswers,
): boolean {
  try {
    sessionStorage.setItem(storageKey(recordId, action), JSON.stringify(answers))
    return true
  } catch {
    return false
  }
}

export function loadReasonAnswers(
  recordId: number,
  action: RecordStatusChangeAction,
): ReasonAnswers | undefined {
  try {
    const json = sessionStorage.getItem(storageKey(recordId, action))
    const value: unknown = json ? JSON.parse(json) : undefined
    if (!value || typeof value !== 'object') {
      return undefined
    }
    const { reason, note } = value as Record<string, unknown>
    if (!isReason(reason) || (note !== undefined && typeof note !== 'string')) {
      return undefined
    }
    return { reason, note }
  } catch {
    return undefined
  }
}

export function clearReasonAnswers(recordId: number, action: RecordStatusChangeAction): void {
  try {
    sessionStorage.removeItem(storageKey(recordId, action))
  } catch {
    // Nothing to clear when storage is blocked.
  }
}

const subscribe = () => () => undefined

/**
 * The saved answers, read once the page is running in the browser. `isClient` is false while
 * rendering on the server, where nothing has been read yet.
 */
export function useSavedReasonAnswers(recordId: number, action: RecordStatusChangeAction) {
  const isClient = useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  )
  return { isClient, saved: isClient ? loadReasonAnswers(recordId, action) : undefined }
}
