'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import type { RecordStatus, RecordType } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { Radio, Radios } from '@/components/Radios/Radios'

import { buildRecordHref } from '../../_lib/recordLinks'
import {
  isStatusChangeAvailable,
  statusChangeActionOrder,
  statusChangeActions,
  type RecordStatusChangeAction,
} from '../_lib/statusChangeActions'

import type { SubmitEvent } from 'react'

type StatusChangeActionFormProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
  recordStatus: RecordStatus
}

export const StatusChangeActionForm = ({
  organisationId,
  recordId,
  recordType,
  recordStatus,
}: StatusChangeActionFormProps) => {
  const router = useRouter()
  const [selectedAction, setSelectedAction] = useState<RecordStatusChangeAction>()
  const [radioError, setRadioError] = useState(false)
  const availableActions = statusChangeActionOrder
    .filter((action) => isStatusChangeAvailable(action, recordStatus))
    .map((action) => ({ action, ...statusChangeActions[action] }))

  const handleSubmit = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()
    const option = availableActions.find((option) => option.action === selectedAction)
    if (!option) {
      setRadioError(true)
      return
    }

    router.push(
      buildRecordHref(organisationId, recordId, recordType, `change-record-status/${option.path}`),
    )
  }

  return (
    <form onSubmit={handleSubmit}>
      <Radios
        headingLevel={2}
        legend="What would you like to do with this record?"
        name="action"
        error={radioError ? 'Select an option' : undefined}
        value={selectedAction ?? null}
        onChange={(value) => {
          const option = availableActions.find((option) => option.action === value)
          if (option) {
            setSelectedAction(option.action)
            setRadioError(false)
          }
        }}
      >
        {availableActions.map((option) => (
          <Radio key={option.action} label={option.label} value={option.action} />
        ))}
      </Radios>

      <ButtonGroup>
        <Button type="submit" variant="cta">
          Continue
        </Button>
        <Button
          elementType={Link}
          href={buildRecordHref(organisationId, recordId, recordType)}
          variant="secondary"
        >
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  )
}
