'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { UserRole } from '@/client/generated/types.gen'
import { Button, ButtonGroup } from '@/components/Button/Button'

import { changeUserPermissionsAction } from '../_actions/changeUserPermissions'

export interface ChangePermissionsFormProps {
  organisationId: number
  userId: number
  membershipId: number
  currentRole: UserRole
}

export function ChangePermissionsForm({
  organisationId,
  userId,
  membershipId,
  currentRole,
}: ChangePermissionsFormProps) {
  const router = useRouter()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formError, setFormError] = useState<string>()

  const switchedRole = currentRole === 'Standard' ? 'Champion' : 'Standard'

  return (
    <form
      noValidate
      onSubmit={async (event) => {
        event.preventDefault()
        event.stopPropagation()

        setFormError(undefined)
        setIsSubmitting(true)

        const response = await changeUserPermissionsAction(
          organisationId,
          userId,
          membershipId,
          switchedRole,
        )

        if (response.status === 'error') {
          setIsSubmitting(false)
          setFormError(response.message)
          return
        }

        router.push(`/portal/organisations/${organisationId}/manage-user-access/${userId}`)
      }}
    >
      {formError && <p role="alert">{formError}</p>}

      <ButtonGroup>
        <Button buttonType="submit" disabled={isSubmitting} variant="cta">
          {isSubmitting ? 'Saving...' : `Make ${switchedRole.toLowerCase()} user`}
        </Button>

        <Button buttonType="button" variant="secondary" onClick={() => router.back()}>
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  )
}
