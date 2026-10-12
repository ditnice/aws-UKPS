import {
  getUserDetailsWithinOrganisation,
  getUserRegistrationById,
} from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { Alert, type AlertProps } from '@/components/Alert/Alert'

import { roleLabels } from '../_lib/userLabels'

import type { RegisteredUserResult, RequestResult, UserActionResult } from '../_lib/userActionAlert'

type RegisteredUserActionProps = {
  organisationId: number
  userAction: RegisteredUserResult
  dismissHref?: AlertProps['dismissHref']
}
const renderRegisteredUserAction = async ({
  organisationId,
  userAction,
  dismissHref,
}: RegisteredUserActionProps) => {
  const { data: user } = await getUserDetailsWithinOrganisation({
    client: await createServerApiClient(),
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
        <Alert type="success">
          <h3>{user?.workEmail ?? 'The user'}&apos;s account has been deactivated</h3>
          <p>We&#39;ve sent an email to {user?.workEmail ?? 'the user'} notifying them.</p>
        </Alert>
      )
    case 'reactivated':
      return (
        <Alert type="success">
          <h3>{user?.workEmail ?? 'The user'}&apos;s account has been reactivated</h3>
          <p>We&rsquo;ve sent an email to {user?.workEmail ?? 'the user'} notifying them.</p>
        </Alert>
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
    case 'removed':
      return (
        <Alert type="success" dismissHref={dismissHref}>
          <h3>User removed from UK PharmaScan</h3>
          <p>
            This user&apos;s personal information is no longer available. In a record&apos;s
            timeline, they now appear as {user?.fullName ?? 'an anonymised user'}.
            <br />
            Records they created can still be accessed and are not affected by removing this user.
          </p>
        </Alert>
      )
  }
}

type UserRequestActionProps = {
  organisationId: number
  userAction: RequestResult
}
const renderUserRequestAction = async ({ organisationId, userAction }: UserRequestActionProps) => {
  const { data: request } = await getUserRegistrationById({
    client: await createServerApiClient(),
    path: { organisationId, id: userAction.userRequestId },
  })
  switch (userAction.action) {
    case 'approved-request':
      return (
        <Alert type="success">
          <h3>Approval Email Sent</h3>
          <p>
            We&rsquo;ve sent an email to {request?.workEmail ?? 'the new user'} notifying them that
            their request has been approved and instructions to set up an account.
          </p>
        </Alert>
      )
    case 'rejected-request':
      return (
        <Alert type="success">
          <h3>Rejection Email Sent</h3>
          <p>
            We&rsquo;ve sent an email to {request?.workEmail ?? 'the user'} notifying them that
            their request has been rejected.
          </p>
        </Alert>
      )
  }
}

type UserActionAlertProps = {
  organisationId: number
} & (
  | {
      userAction: Omit<RegisteredUserResult, 'action'> & { action: 'removed' }
      dismissHref?: AlertProps['dismissHref']
    }
  | {
      userAction: UserActionResult
      dismissHref?: never
    }
)
export function UserActionAlert({ organisationId, userAction, dismissHref }: UserActionAlertProps) {
  return userAction.type === 'user'
    ? renderRegisteredUserAction({ organisationId, userAction, dismissHref })
    : renderUserRequestAction({ organisationId, userAction })
}
