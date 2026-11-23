'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import type { RecordStatusChangeReason, RecordType } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { Radio, Radios } from '@/components/Radios/Radios'
import { Textarea } from '@/components/Textarea/Textarea'

import { buildRecordHref } from '../../_lib/recordLinks'
import {
  clearReasonAnswers,
  saveReasonAnswers,
  useSavedReasonAnswers,
  type ReasonAnswers,
} from '../_lib/reasonAnswerStorage'
import {
  buildStatusChangeHref,
  statusChangeErrorMessages,
  type RecordStatusChangeAction,
} from '../_lib/statusChangeActions'

import type { ChangeEvent, SubmitEvent } from 'react'

export type StatusChangeReasonOption<TReason extends RecordStatusChangeReason> = {
  reason: TReason
  label: string
  hint: string
}

export type StatusChangeReasonFormProps<TReason extends RecordStatusChangeReason> = {
  organisationId: number
  recordId: number
  recordType: RecordType
  action: RecordStatusChangeAction
  legend: string
  missingReasonError: string
  options: readonly StatusChangeReasonOption<TReason>[]
}

/**
 * Asks for the reason for a status change and saves it for the confirmation page. Choosing Other
 * reveals a text box for the detail, which is sent as the note. Answers saved earlier, such as
 * when the user comes back from the confirmation page, are shown again.
 */
export const StatusChangeReasonForm = <TReason extends RecordStatusChangeReason>(
  props: StatusChangeReasonFormProps<TReason>,
) => {
  const { isClient, saved } = useSavedReasonAnswers(props.recordId, props.action)
  const savedReason = props.options.find((option) => option.reason === saved?.reason)?.reason

  // Remounted once the saved answers can be read, so they become the form's starting values.
  return (
    <ReasonForm
      key={isClient ? 'client' : 'server'}
      {...props}
      initialReason={savedReason}
      initialDetail={savedReason === 'Other' ? (saved?.note ?? '') : ''}
    />
  )
}

type ReasonFormProps<TReason extends RecordStatusChangeReason> =
  StatusChangeReasonFormProps<TReason> & {
    initialReason: TReason | undefined
    initialDetail: string
  }

const ReasonForm = <TReason extends RecordStatusChangeReason>({
  organisationId,
  recordId,
  recordType,
  action,
  legend,
  missingReasonError,
  options,
  initialReason,
  initialDetail,
}: ReasonFormProps<TReason>) => {
  const router = useRouter()
  const [selectedReason, setSelectedReason] = useState<TReason | undefined>(initialReason)
  const [radioError, setRadioError] = useState(false)
  const [otherDetail, setOtherDetail] = useState(initialDetail)
  const [otherDetailError, setOtherDetailError] = useState(false)
  const [saveFailed, setSaveFailed] = useState(false)

  const handleSubmit = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!selectedReason) {
      setRadioError(true)
      return
    }

    const note = selectedReason === 'Other' ? otherDetail.trim() : undefined
    if (note === '') {
      setOtherDetailError(true)
      return
    }

    const answers: ReasonAnswers = { reason: selectedReason, note }
    if (!saveReasonAnswers(recordId, action, answers)) {
      setSaveFailed(true)
      return
    }

    router.push(buildStatusChangeHref(organisationId, recordId, recordType, action, 'confirm'))
  }

  return (
    <form onSubmit={handleSubmit}>
      {saveFailed && (
        <ErrorState>There was a problem saving your answer. Please try again later.</ErrorState>
      )}

      <Radios
        headingLevel={2}
        legend={legend}
        name="reason"
        error={radioError ? missingReasonError : undefined}
        value={selectedReason ?? null}
        onChange={(value) => {
          const option = options.find((option) => option.reason === value)
          if (option) {
            setSelectedReason(option.reason)
            setRadioError(false)
          }
        }}
      >
        {options.map((option) => (
          <Radio
            key={option.reason}
            label={option.label}
            value={option.reason}
            hint={option.hint}
            conditional={
              option.reason === 'Other' ? (
                <Textarea
                  label="Provide additional information"
                  name={`${action}-other-detail`}
                  value={otherDetail}
                  onChange={(event: ChangeEvent<HTMLTextAreaElement>) => {
                    setOtherDetail(event.target.value)
                    setOtherDetailError(false)
                  }}
                  error={otherDetailError}
                  errorMessage={statusChangeErrorMessages.noteRequired}
                />
              ) : undefined
            }
          />
        ))}
      </Radios>

      <ButtonGroup>
        <Button type="submit" variant="cta">
          Continue
        </Button>
        <Button
          elementType={Link}
          href={buildRecordHref(organisationId, recordId, recordType)}
          onClick={() => clearReasonAnswers(recordId, action)}
          variant="secondary"
        >
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  )
}
