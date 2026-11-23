'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useEffect, useState } from 'react'

import type { RecordType } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { buildRecordHref } from '../../_lib/recordLinks'
import { clearReasonAnswers, useSavedReasonAnswers } from '../_lib/reasonAnswerStorage'
import {
  buildStatusChangeHref,
  getStatusChangeErrorMessage,
  statusChangeActions,
  submitStatusChange,
  type RecordStatusChangeAction,
} from '../_lib/statusChangeActions'

export type ConfirmStatusChangeControlsProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
  action: RecordStatusChangeAction
}

/**
 * Makes the status change with the answers saved on the reason page, then returns to the record.
 * Without saved answers, such as when opened from a link, the user is sent to the reason page.
 */
export const ConfirmStatusChangeControls = ({
  organisationId,
  recordId,
  recordType,
  action,
}: ConfirmStatusChangeControlsProps) => {
  const router = useRouter()
  const { isClient, saved } = useSavedReasonAnswers(recordId, action)
  const [submitError, setSubmitError] = useState<string>()
  const [submitting, setSubmitting] = useState(false)
  // Set when the saved answers are cleared on the way out, so that isn't mistaken for arriving
  // without any.
  const [leaving, setLeaving] = useState(false)
  const reasonHref = buildStatusChangeHref(organisationId, recordId, recordType, action, 'reason')
  const hasSaved = saved !== undefined

  useEffect(() => {
    if (isClient && !hasSaved && !leaving) {
      router.replace(reasonHref)
    }
  }, [isClient, hasSaved, leaving, reasonHref, router])

  const leave = () => {
    setLeaving(true)
    clearReasonAnswers(recordId, action)
  }

  const confirm = async () => {
    if (!saved) {
      return
    }

    setSubmitError(undefined)
    setSubmitting(true)
    try {
      const { error, response } = await submitStatusChange(action, {
        organisationId,
        recordId,
        ...saved,
      })
      if (response?.ok) {
        leave()
        router.push(
          buildRecordHref(
            organisationId,
            recordId,
            recordType,
            undefined,
            statusChangeActions[action].changedAlert,
          ),
        )
        return
      }
      setSubmitError(getStatusChangeErrorMessage(response?.status, error))
    } catch {
      setSubmitError(getStatusChangeErrorMessage(undefined, undefined))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <>
      {submitError && <ErrorState>{submitError}</ErrorState>}
      <ButtonGroup>
        <Button variant="cta" disabled={!hasSaved || submitting} onClick={() => void confirm()}>
          Confirm
        </Button>
        <Button
          elementType={Link}
          href={buildRecordHref(organisationId, recordId, recordType)}
          onClick={leave}
          variant="secondary"
        >
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}
