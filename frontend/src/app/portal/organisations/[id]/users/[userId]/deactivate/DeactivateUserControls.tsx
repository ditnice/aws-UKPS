'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { deactivateMembership } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

export type DeactivateUserControlsProps = {
  organisationId: number
  membershipId: number
}
const DeactivateUserControls = ({ organisationId, membershipId }: DeactivateUserControlsProps) => {
  const router = useRouter()
  const [hasError, setHasError] = useState(false)
  const deactivateUser = async () => {
    setHasError(true)
    const result = await deactivateMembership({
      path: { organisationId, membershipId: membershipId },
    })
    if (result.error) {
      setHasError(true)
    }
    router.push(`/portal/organisations/${organisationId}`)
  }
  return (
    <>
      {hasError && (
        <ErrorState data-testid="action-error">
          A error occurred when attempting to deactivate the user.
        </ErrorState>
      )}
      <ButtonGroup>
        <Button data-testid="action-button" variant="cta" onClick={deactivateUser}>
          Deactivate User
        </Button>
        <Button data-testid="cancel-button" variant="secondary" onClick={router.back}>
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}

export default DeactivateUserControls
