import type { Client } from '@/client/generated/client'
import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import { Alert } from '@/components/Alert/Alert'

import { roleLabels } from '../_lib/userLabels'

import type { UserActionResult } from '../_lib/userActionAlert'

interface UserActionAlertProps {
  apiClient: Client
  organisationId: number
  userAction: UserActionResult
}

export async function UserActionAlert({
  apiClient,
  organisationId,
  userAction,
}: UserActionAlertProps) {
  const { data: user } = await getUserDetailsWithinOrganisation({
    client: apiClient,
    path: { organisationId, userId: userAction.userId },
  })

  switch (userAction.action) {
    case 'invited':
      return (
        <Alert type="success">
          <h3>Invitation sent</h3>
          <p>
            We&rsquo;ve sent an email to {user?.workEmail ?? 'the new user'} with instructions to
            set up an account.
          </p>
        </Alert>
      )
    case 'deactivated':
      return (
        <>
          <Alert type="success">
            <h3>{user ? `${user.workEmail}'s` : 'An'} account has been deactivated</h3>
            <p>
              A user {user && `with the email ${user.workEmail}`} has been successfully deactivated.
            </p>
          </Alert>
        </>
      )
    case 'permissions-updated':
      return (
        <Alert type="success">
          <h3>Permissions changed</h3>
          <p>
            {user?.workEmail ?? "The user's"}{' '}
            {user
              ? `is now a ${roleLabels[user.userRole].toLowerCase()}`
              : 'permissions have been updated'}
            .
          </p>
        </Alert>
      )
  }
}
