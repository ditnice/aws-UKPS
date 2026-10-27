'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { discardRecord } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { buildRecordActionHref } from '../../../_lib/recordActionAlert'

export type DiscardRecordControlsProps = {
  organisationId: number
  recordId: number
}

export const DiscardRecordControls = ({ organisationId, recordId }: DiscardRecordControlsProps) => {
  const router = useRouter()
  const [hasError, setHasError] = useState(false)
  const [loading, setLoading] = useState(false)

  const discardHandler = async () => {
    setHasError(false)
    setLoading(true)

    try {
      const result = await discardRecord({ path: { recordId } })
      if (result.error) {
        setHasError(true)
        return
      }
      router.push(buildRecordActionHref(organisationId, 'discarded'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      {hasError && (
        <ErrorState>There was a problem discarding this record. Please try again later.</ErrorState>
      )}
      <ButtonGroup>
        <Button variant="cta" disabled={loading} onClick={discardHandler}>
          Confirm
        </Button>
        <Button variant="secondary" disabled={loading} onClick={() => router.back()}>
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}
