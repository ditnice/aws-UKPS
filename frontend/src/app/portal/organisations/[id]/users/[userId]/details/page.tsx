import { getUserDetailsWithinOrganisation } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { UserAuthenticationDetails } from '@/lib/users/UserAuthenticationDetails'
import { UserDetails } from '@/lib/users/UserDetails'

import {
  buildViewUserDetailsHref,
  PageParams,
  parseTypedParams,
  requestedNewEmailFlag,
  updatedFlag,
} from './navigation'

const Page = async (params: PageParams) => {
  const { userId, organisationId, updated, requestedNewEmail } = await parseTypedParams(params)
  const unchangingParams = { userId, organisationId }
  const apiClient = await createServerApiClient()
  const { data: user, error } = await getUserDetailsWithinOrganisation({
    client: apiClient,
    path: { userId, organisationId },
  })

  return (
    <>
      {updated && (
        <Alert type="success">
          <h3>User details changed successfully</h3>
          <p>The user&rsquo;s details have been updated.</p>
        </Alert>
      )}
      {requestedNewEmail && (
        <Alert type="success">
          <h3>Email update requested</h3>
          <p>
            We&rsquo;ve sent a verification email to the user&rsquo;s new address. They need to use
            the link in the email to verify it. Their email address won&rsquo;t change until they
            verify it and log in with it.
          </p>
        </Alert>
      )}
      <PageHeader backLink={<BackLinkBrowser />} heading="Manage details" />
      {!user || error ? (
        <ErrorState>Failed to retrieve the user.</ErrorState>
      ) : (
        <>
          <UserAuthenticationDetails currentUser={user} />
          <UserDetails
            currentUser={user}
            navigation={{
              returnLink: buildViewUserDetailsHref({ ...unchangingParams }),
              onSuccessFlag: {
                fullName: updatedFlag,
                contactNumber: updatedFlag,
                email: requestedNewEmailFlag,
              },
            }}
          />
        </>
      )}
    </>
  )
}

export default Page
