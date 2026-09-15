'use client'

import Link from 'next/link'
import { useState } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import { Button } from '@/components/Button/Button'

interface Props {
  organisationId: number
  selectedUserId: number
  userRole: string
}

export function ManageUserAccessActions({ organisationId, selectedUserId, userRole }: Props) {
  const [selectedAction, setSelectedAction] = useState('')
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
  return (
    <>
      <FormGroup name="manage user's access">
        <div>
          <Radio
            label="Change user permissions"
            value="Change user permissions"
            hint="Change what the user can do"
            name="action"
            onChange={() => setSelectedAction('Change user permissions')}
          />
          <Radio
            label="Deactivate user"
            value="Deactivate user"
            hint="Temporarily remove the user's access to the system"
            name="action"
            onChange={() => setSelectedAction('Deactivate user')}
          />

          {userRole === 'Super' ? (
            <Radio
              label="Remove user  - not implemented yet"
              value="Remove user"
              hint="Permanently remove the user's access to the system"
              name="action"
              onChange={() => setSelectedAction('Remove user')}
            />
          ) : null}

          <Radio
            label="Manage user details and sign in method  - not implemented yet"
            value="Manage user details and sign in method"
            hint="Update user details or sign in method"
            name="action"
            onChange={() => setSelectedAction('Manage user details and sign in method')}
          />
        </div>
      </FormGroup>
      <Button elementType={Link} variant="cta" href={continueHref} aria-disabled={!selectedAction}>
        Continue
      </Button>
    </>
  )
}
