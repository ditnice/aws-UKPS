import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import { UserDetails } from './_components/UserDetails'

export const dynamic = 'force-dynamic'

export default async function Me() {
  const apiClient = await createServerApiClient()
  const { data: me, error } = await getUsersMe({
    client: apiClient,
  })

  return (
    <>
      <PageHeader backLink={<BackLinkBrowser />} heading="Your details" />
      {!me || error ? (
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      ) : (
        <UserDetails currentUser={me}></UserDetails>
      )}
    </>
  )
}
