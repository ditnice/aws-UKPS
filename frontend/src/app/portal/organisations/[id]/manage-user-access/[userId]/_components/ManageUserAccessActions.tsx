'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import { Button } from '@/components/Button/Button'

interface Props {
  organisationId: number
  selectedUserId: number
  currentUserRole: string
}

type Actions =
  | 'Change user permissions'
  | 'Deactivate user'
  | 'Remove user'
  | 'Manage user details and sign in method'

export default function ManageUserAccessActions({
  organisationId,
  selectedUserId,
  currentUserRole,
}: Props) {
  const router = useRouter()
  const [selectedAction, setSelectedAction] = useState<Actions | undefined>()
  const [radioError, setRadioError] = useState(false)

  const getContinueHref = (selectedAction: Actions | undefined) => {
    switch (selectedAction) {
      case 'Change user permissions':
        return `/portal/organisations/${organisationId}/manage-user-access/${selectedUserId}/change-permissions`

      case 'Deactivate user':
        return `/portal/organisations/${organisationId}/users/${selectedUserId}/deactivate`

      case 'Remove user':
        return `/portal/organisations/${organisationId}/users/${selectedUserId}/remove`

      case 'Manage user details and sign in method':
        return `/portal/organisations/${organisationId}/users/${selectedUserId}/manage-details`

      default:
        return '#'
    }
  }
  const continueHref = getContinueHref(selectedAction)
  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!selectedAction) {
      setRadioError(true)
      return
    }
    router.push(continueHref)
  }

  const radios = [
    <Radio
      key="change-permissions"
      label="Change user permissions"
      value="Change user permissions"
      hint="Change what the user can do"
      name="action"
      onChange={() => {
        setSelectedAction('Change user permissions')
        setRadioError(false)
      }}
    />,

    <Radio
      key="deactivate"
      label="Deactivate user"
      value="Deactivate user"
      hint="Temporarily remove the user's access to the system"
      name="action"
      onChange={() => {
        setSelectedAction('Deactivate user')
        setRadioError(false)
      }}
    />,
    currentUserRole === 'Super'
      ? [
          <Radio
            key="remove"
            label="Remove user - not implemented yet"
            value="Remove user"
            hint="Permanently remove the user's access to the system"
            name="action"
            onChange={() => {
              setSelectedAction('Remove user')
              setRadioError(false)
            }}
          />,
        ]
      : [],

    <Radio
      key="manage"
      label="Manage user details and sign in method  - not implemented yet"
      value="Manage user details and sign in method"
      hint="Update user details or sign in method"
      name="action"
      onChange={() => {
        setSelectedAction('Manage user details and sign in method')
        setRadioError(false)
      }}
    />,
  ]
  return (
    <form onSubmit={handleSubmit}>
      <FormGroup
        name="manage user's access"
        groupError={radioError ? 'Select an option - No answer provided' : undefined}
      >
        {radios}
      </FormGroup>
      <Button type="submit" variant="cta" href={continueHref}>
        Continue
      </Button>
    </form>
  )
}
