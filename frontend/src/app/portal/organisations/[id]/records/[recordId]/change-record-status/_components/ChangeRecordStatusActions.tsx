'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import type { RecordStatus, RecordType } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { Radio, Radios } from '@/components/Radios/Radios'

import { buildRecordHref } from '../../_lib/recordLinks'

type ChangeRecordStatusProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
  recordStatus: RecordStatus
}

type RecordStatusAction = 'Archive' | 'PlaceOnHold' | 'RemoveHold'

// TODO: Action pages to continue to not yet built, destination pages will come from URP-380, URP-382 and URP-419, placeholder paths provided here
const actionOptions: {
  action: RecordStatusAction
  label: string
  path: string
  isAvailable: (status: RecordStatus) => boolean
}[] = [
  {
    action: 'Archive',
    label: 'Archive the record',
    path: 'archive',
    isAvailable: () => true,
  },
  {
    action: 'PlaceOnHold',
    label: 'Place the record on hold',
    path: 'place-on-hold',
    isAvailable: (status) => status === 'Published',
  },
  {
    action: 'RemoveHold',
    label: 'Remove the record hold',
    path: 'remove-hold',
    isAvailable: (status) => status === 'OnHold',
  },
]

export const ChangeRecordStatusActions = ({
  organisationId,
  recordId,
  recordType,
  recordStatus,
}: ChangeRecordStatusProps) => {
  const router = useRouter()
  const [selectedAction, setSelectedAction] = useState<RecordStatusAction>()
  const [radioError, setRadioError] = useState(false)
  const availableActions = actionOptions.filter((option) => option.isAvailable(recordStatus))

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
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
