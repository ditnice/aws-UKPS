import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

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
          <h3>Your details have been updated</h3>
        </Alert>
      )}
      <PageHeader backLink={<BackLinkBrowser />} heading="Your details" />
      {!me || error ? (
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      ) : (
        <UserDetails currentUser={me}></UserDetails>
      )}
    </>
  )
}
