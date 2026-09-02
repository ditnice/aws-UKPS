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

  return userAction.action === 'invited' ? (
    <Alert type="success">
      <h3>Invitation sent</h3>
      <p>
        We&rsquo;ve sent an email to {user?.workEmail ?? 'the new user'} with instructions to set up
        an account.
      </p>
    </Alert>
  ) : (
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
