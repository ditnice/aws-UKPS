import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import { UserAuthenticationDetails } from './_components/UserAuthenticationDetails'
import { UserDetails } from './_components/UserDetails'

export const dynamic = 'force-dynamic'

export default async function Me({
  searchParams,
}: {
  searchParams: Promise<{ updated?: string }>
}) {
  const { updated: updateString } = await searchParams
  const updated = updateString?.toLocaleLowerCase() === `${true}`
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
      <PageHeader backLink={<BackLinkBrowser />} heading="Manage details" />
      {!me || error ? (
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      ) : (
        <>
          <UserAuthenticationDetails currentUser={me} />
          <UserDetails currentUser={me} />
        </>
      )}
    </>
  )
}
