'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import type { RecordStatus, RecordType } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'

import { buildRecordHref } from '../../_lib/recordActions'

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
  hint: string
  path: string
}[] = [
  {
    action: 'Archive',
    label: 'Archive the record',
    hint: 'Development has ended and will not restart. The record cannot be updated once it has been archived.',
    path: 'archive',
  },
  {
    action: 'PlaceOnHold',
    label: 'Place the record on hold',
    hint: 'Development has paused and may restart in the future. Update requests for this record will be temporarily paused.',
    path: 'place-on-hold',
  },
  {
    action: 'RemoveHold',
    label: 'Remove the record hold',
    hint: 'Development has restarted and future updates will be required.',
    path: 'remove-hold',
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
  const availableActions = actionOptions.filter((option) => {
    switch (option.action) {
      case 'PlaceOnHold':
        return recordStatus === 'Published'
      case 'RemoveHold':
        return recordStatus === 'OnHold'
      default:
        return true
    }
  })

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const option = availableActions.find((option) => option.action === selectedAction)
    if (!option) {
      setRadioError(true)
      return
    }

    router.push(
      `/portal/organisations/${organisationId}/records/${recordId}/change-record-status/${option.path}?recordType=${recordType}`,
    )
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGroup
        name="manage record status"
        groupError={radioError ? 'Select an option' : undefined}
      >
        {availableActions.map((option) => (
          <Radio
            key={option.action}
            label={option.label}
            value={option.action}
            hint={option.hint}
            name="action"
            onChange={() => {
              setSelectedAction(option.action)
              setRadioError(false)
            }}
          />
        ))}
      </FormGroup>

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
