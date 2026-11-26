import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'
import { UserAuthenticationDetails } from '@/lib/users/UserAuthenticationDetails'
import { UserDetails } from '@/lib/users/UserDetails'

import {
  buildMyDetailsHref,
  parseTypedParamsFromSearchParams,
  requestedNewEmailFlag,
  SearchParams,
  updatedFlag,
} from './navigation'
import { buildUpdateUserMfaLink } from './update-mfa/navigate'

export const dynamic = 'force-dynamic'

const mfaUpdatedMessages = {
  primary: 'Your primary authentication method has been updated.',
  secondary: 'Your backup authentication method has been updated.',
} as const

const authenticationActions = {
  changePassword: '#change-password',
  sendRecoveryLink: {
    primary: '#send-recovery-link',
    secondary: '#send-backup-method-recovery-link',
  },
  changeMfa: {
    primary: buildUpdateUserMfaLink(),
    secondary: '#update-backup-authentication-method',
  },
}

export default async function Me({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const { updated, requestedNewEmail, mfaUpdated } = parseTypedParamsFromSearchParams(
    await searchParams,
  )
  const apiClient = await createServerApiClient()
  const { data: me, error } = await getUsersMe({
    client: apiClient,
  })

  return (
    <>
      {updated && (
        <Alert type="success">
          <h3>Details changed successfully</h3>
          <p>You can update your details at any time from manage my details.</p>
        </Alert>
      )}
      {requestedNewEmail && (
        <Alert type="success">
          <h3>Email update requested</h3>
          <p>
            We&rsquo;ve sent a verification email to your new address. Check your inbox and use the
            link in the email to verify it. Once it&rsquo;s verified, log in with it to complete the
            change.
          </p>
        </Alert>
      )}
      {mfaUpdated && (
        <Alert type="success">
          <h3>Two-step verification updated</h3>
          <p>{mfaUpdatedMessages[mfaUpdated]}</p>
          <p>You can change your security settings at any time from manage my details.</p>
        </Alert>
      )}
      <PageHeader backLink={<BackLinkBrowser />} heading="Manage details" />
      {!me || error ? (
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      ) : (
        <>
          <UserAuthenticationDetails currentUser={me} actions={authenticationActions} />
          <UserDetails
            currentUser={me}
            navigation={{
              returnLink: buildMyDetailsHref(),
              onSuccessFlag: {
                contactNumber: updatedFlag,
                fullName: updatedFlag,
                email: requestedNewEmailFlag,
              },
            }}
          />
        </>
      )}
    </>
  )
}
