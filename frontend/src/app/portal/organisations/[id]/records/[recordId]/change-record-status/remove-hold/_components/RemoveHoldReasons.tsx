'use client'

import Link from 'next/link'
import { useState } from 'react'

import type { RecordType } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { Radio, Radios } from '@/components/Radios/Radios'

import { buildRecordHref } from '../../../_lib/recordLinks'

import type { SubmitEvent } from 'react'

type RemoveHoldReasonsProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
}

// Mirrors the backend RecordStatusChangeReason values that are valid when removing a hold.
// TODO: replace with the generated type once the record status API exposes it.
type RemoveHoldReason = 'TrialResumed' | 'PlansRefiled' | 'Other'

const reasonOptions: { reason: RemoveHoldReason; label: string; hint: string }[] = [
  {
    reason: 'TrialResumed',
    label: 'Trial has resumed',
    hint: 'One or more clinical trials have restarted.',
  },
  {
    reason: 'PlansRefiled',
    label: 'Regulatory plans have been re-filed to MHRA',
    hint: 'A submission to MHRA has been re-filed and development will be continuing.',
  },
  {
    reason: 'Other',
    label: 'Other',
    hint: 'Development has restarted for another reason, for example, manufacturing, funding, or strategic considerations have changed.',
  },
]

export const RemoveHoldReasons = ({
  organisationId,
  recordId,
  recordType,
}: RemoveHoldReasonsProps) => {
  const [selectedReason, setSelectedReason] = useState<RemoveHoldReason>()
  const [radioError, setRadioError] = useState(false)

  const handleSubmit = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!selectedReason) {
      setRadioError(true)
      return
    }

    // TODO: submit the hold removal once the record status API exists.
  }

  return (
    <form onSubmit={handleSubmit}>
      <Radios
        headingLevel={2}
        legend="What is the reason for removing hold?"
        name="reason"
        error={radioError ? 'Select a reason for removing the hold' : undefined}
        value={selectedReason ?? null}
        onChange={(value) => {
          const option = reasonOptions.find((option) => option.reason === value)
          if (option) {
            setSelectedReason(option.reason)
            setRadioError(false)
          }
        }}
      >
        {reasonOptions.map((option) => (
          <Radio
            key={option.reason}
            label={option.label}
            value={option.reason}
            hint={option.hint}
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
          variant="secondary"
        >
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  )
}
