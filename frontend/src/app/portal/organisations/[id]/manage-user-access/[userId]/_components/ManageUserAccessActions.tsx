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
  return (
    <>
      <FormGroup name="manage user's access">
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

        <Radio // only if super so add in validation
          label="Remove user"
          value="Remove user"
          hint="Permanently remove the user's access to the system"
          name="action"
          onChange={() => setSelectedAction('Remove user')}
        />

        <Radio
          label="Manage user details and sign in method"
          value="Manage user details and sign in method"
          hint="Update user details or sign in method"
          name="action"
          onChange={() => setSelectedAction('Manage user details and sign in method')}
        />
      </FormGroup>

      {selectedAction === 'Change user permissions' && (
        <Button
          elementType={Link}
          variant="cta"
          href={`/portal/organisations/${organisationId}/manage-user-access/${selectedUserId}/change-permissions`}
        >
          Continue
        </Button>
      )}

      {selectedAction === 'Deactivate user' && (
        <Button
          elementType={Link}
          variant="cta"
          href={`/portal/organisations/${organisationId}/users/${selectedUserId}/deactivate`}
        >
          Continue
        </Button>
      )}

      {selectedAction === 'Remove user' && (
        <Button
          elementType={Link}
          variant="cta"
          href={`/portal/organisations/${organisationId}/users/${selectedUserId}/remove`}
        >
          Continue
        </Button>
      )}

      {selectedAction === 'Manage user details and sign in method' && (
        <Button
          elementType={Link}
          variant="cta"
          href={`/portal/organisations/${organisationId}/users/${selectedUserId}/manage-details`}
        >
          Continue
        </Button>
      )}
    </>
  )
}
