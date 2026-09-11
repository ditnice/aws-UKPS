'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { reactivateMembership } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { buildUserActionHref } from '../../../_lib/userActionAlert'

export type ReactivateUserControlsProps = {
  organisationId: number
  userId: number
  membershipId: number
}
const ReactivateUserControls = ({
  organisationId,
  userId,
  membershipId,
}: ReactivateUserControlsProps) => {
  const router = useRouter()
  const [hasError, setHasError] = useState(false)
  const [loading, setLoading] = useState(false)
  const reactivateUser = async () => {
    setHasError(false)
    setLoading(true)

    try {
      const result = await reactivateMembership({
        path: { organisationId, membershipId: membershipId },
      })
      if (result.error) {
        setHasError(true)
        return
      }
      router.push(buildUserActionHref(organisationId, 'reactivated', userId))
    } finally {
      setLoading(false)
    }
  }
  return (
    <>
      {hasError && (
        <ErrorState data-testid="action-error">
          A error occurred when attempting to reactivate the user.
        </ErrorState>
      )}
      <ButtonGroup>
        <Button
          data-testid="action-button"
          variant="cta"
          disabled={loading}
          onClick={reactivateUser}
        >
          Reactivate User
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

export default ReactivateUserControls
