'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { Button, ButtonGroup } from '@/components/Button/Button'

import { getSwitchedRole, switchRoleButtonLabels, type SwitchableRole } from '../../_lib/userRoles'
import { changeUserPermissionsAction } from '../_actions/changeUserPermissions'

export interface ChangePermissionsFormProps {
  organisationId: number
  userId: number
  membershipId: number
  currentRole: SwitchableRole
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

  const manageUserAccessHref = `/portal/organisations/${organisationId}/manage-user-access/${userId}`

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
          getSwitchedRole(currentRole),
        )

        if (response.status === 'error') {
          setIsSubmitting(false)
          setFormError(response.message)
          return
        }

        router.push(manageUserAccessHref)
      }}
    >
      {formError && <p role="alert">{formError}</p>}

      <ButtonGroup>
        <Button buttonType="submit" disabled={isSubmitting} variant="cta">
          {isSubmitting ? 'Saving...' : switchRoleButtonLabels[currentRole]}
        </Button>

        <Button buttonType="button" variant="secondary" onClick={() => router.back()}>
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  )
}
