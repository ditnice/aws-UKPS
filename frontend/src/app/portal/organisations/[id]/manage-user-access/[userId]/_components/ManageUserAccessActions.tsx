'use client'

import Link from 'next/link'
import router from 'next/router'
import { useState } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import { Button } from '@/components/Button/Button'

interface Props {
  organisationId: number
  selectedUserId: number
  currentUserRole: string
}

export function ManageUserAccessActions({
  organisationId,
  selectedUserId,
  currentUserRole,
}: Props) {
  const [selectedAction, setSelectedAction] = useState('')
  const [radioError, setRadioError] = useState(false)
  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault()

    if (!selectedAction) {
      setRadioError(true)
      return
    }
    router.push(continueHref)
  }
  const continueHref =
    selectedAction === 'Change user permissions'
      ? `/portal/organisations/${organisationId}/manage-user-access/${selectedUserId}/change-permissions`
      : selectedAction === 'Deactivate user'
        ? `/portal/organisations/${organisationId}/users/${selectedUserId}/deactivate`
        : selectedAction === 'Remove user'
          ? `/portal/organisations/${organisationId}/users/${selectedUserId}/remove`
          : selectedAction === 'Manage user details and sign in method'
            ? `/portal/organisations/${organisationId}/users/${selectedUserId}/manage-details`
            : '#'
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
