'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { deactivateMembership } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { buildUserActionHref } from '../../../_lib/userActionAlert'

export type DeactivateUserControlsProps = {
  organisationId: number
  userId: number
  membershipId: number
}
const DeactivateUserControls = ({
  organisationId,
  userId,
  membershipId,
}: DeactivateUserControlsProps) => {
  const router = useRouter()
  const [hasError, setHasError] = useState(false)
  const [loading, setLoading] = useState(false)
  const deactivateUser = async () => {
    setHasError(false)
    setLoading(true)

    try {
      const result = await deactivateMembership({
        path: { organisationId, membershipId: membershipId },
      })
      if (result.error) {
        setHasError(true)
        return
      }
      router.push(buildUserActionHref(organisationId, 'deactivated', userId))
    } finally {
      setLoading(false)
    }
  }
  return (
    <>
      {hasError && (
        <ErrorState data-testid="action-error">
          A error occurred when attempting to deactivate the user.
        </ErrorState>
      )}
      <ButtonGroup>
        <Button
          data-testid="action-button"
          variant="cta"
          disabled={loading}
          onClick={deactivateUser}
        >
          Deactivate User
        </Button>
        <Button
          data-testid="cancel-button"
          variant="secondary"
          disabled={loading}
          onClick={() => router.back()}
        >
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}

export default DeactivateUserControls
