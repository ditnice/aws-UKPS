'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import type { UserMembershipAction, UserOrgStatus, UserRole } from '@/client/generated/types.gen'
import { Button } from '@/components/Button/Button'

interface Props {
  organisationId: number
  selectedUserId: number
  currentUserRole: UserRole
  selectedUserRole: UserRole
  status: UserOrgStatus
}

const actionOptions: {
  action: UserMembershipAction
  label: string
  hint: string
  path: string
}[] = [
  {
    action: 'EditUserRole',
    label: 'Change user permissions',
    hint: 'Change the user role to either champion user or standard user',
    path: 'change-permissions',
  },
  {
    action: 'DeactivateMembership',
    label: 'Deactivate user',
    hint: "Temporarily remove the user's access to the system",
    path: 'deactivate',
  },
  {
    action: 'ReactivateMembership',
    label: 'Reactivate user',
    hint: "Restore the user's access to the organisation",
    path: 'reactivate',
  },
  {
    action: 'RemoveUser',
    label: 'Remove user',
    hint: "Permanently remove the user's access to the system",
    path: 'remove',
  },
]

export default function ManageUserAccessActions({
  organisationId,
  selectedUserId,
  currentUserRole,
  selectedUserRole,
  status,
}: Props) {
  const router = useRouter()
  const [selectedAction, setSelectedAction] = useState<UserMembershipAction>()
  const [radioError, setRadioError] = useState(false)
  // UI guidance only: the API independently authorises and validates each operation.
  const canManage = currentUserRole === 'Super' || currentUserRole === 'Champion'
  const isActive = status === 'Active' || status === 'Inactive'
  const availableActions = actionOptions.filter((option) => {
    if (!canManage) return false
    switch (option.action) {
      case 'EditUserRole':
        return isActive && selectedUserRole !== 'Super'
      case 'DeactivateMembership':
        return isActive
      case 'ReactivateMembership':
        return status === 'Deactivated'
      case 'RemoveUser':
        return (
          currentUserRole === 'Super' &&
          (isActive || status === 'AwaitingSetup' || status === 'Deactivated')
        )
      default:
        return false
    }
  })

  if (!availableActions.length) {
    return <p>No actions are available for this user.</p>
  }

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const option = availableActions.find((option) => option.action === selectedAction)
    if (!option) {
      setRadioError(true)
      return
    }

    const route =
      option.action === 'EditUserRole' || option.action === 'RemoveUser'
        ? 'manage-user-access'
        : 'users'
    router.push(`/portal/organisations/${organisationId}/${route}/${selectedUserId}/${option.path}`)
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGroup
        name="manage user's access"
        groupError={radioError ? 'Select an option - No answer provided' : undefined}
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
      <Button type="submit" variant="cta">
        Continue
      </Button>
    </form>
  )
}
