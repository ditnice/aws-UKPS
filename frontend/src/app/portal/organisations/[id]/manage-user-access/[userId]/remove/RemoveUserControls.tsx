'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { removeUser } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { buildUserActionHref } from '../../../_lib/userActionAlert'

export type RemoveUserControlsProps = {
  organisationId: number
  userId: number
}

const RemoveUserControls = ({ organisationId, userId }: RemoveUserControlsProps) => {
  const router = useRouter()
  const [hasError, setHasError] = useState(false)
  const [loading, setLoading] = useState(false)

  const removeUserHandler = async () => {
    setHasError(false)
    setLoading(true)

    try {
      const result = await removeUser({ path: { userId } })
      if (result.error) {
        setHasError(true)
        return
      }
      router.push(buildUserActionHref(organisationId, { action: 'removed', userId }))
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      {hasError && (
        <ErrorState data-testid="action-error">
          An error occurred when attempting to remove the user.
        </ErrorState>
      )}
      <ButtonGroup>
        <Button
          data-testid="action-button"
          variant="cta"
          disabled={loading}
          onClick={removeUserHandler}
        >
          Remove user
        </Button>
        <Button
          elementType={Link}
          variant="secondary"
          disabled={loading}
          href={`/portal/organisations/${organisationId}/manage-user-access/${userId}`}
        >
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}

export default RemoveUserControls
