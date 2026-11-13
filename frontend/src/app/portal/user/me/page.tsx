import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import { UserAuthenticationDetails } from './_components/UserAuthenticationDetails'
import { UserDetails } from './_components/UserDetails'
import { parseTypedParamsFromSearchParams, SearchParams } from './navigation'

export const dynamic = 'force-dynamic'

const mfaUpdatedMessages = {
  primary: 'Your primary authentication method has been updated.',
  secondary: 'Your backup authentication method has been updated.',
} as const

export default async function Me({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const { updated, mfaUpdated } = parseTypedParamsFromSearchParams(await searchParams)
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
          <UserAuthenticationDetails currentUser={me} />
          <UserDetails currentUser={me} />
        </>
      )}
    </>
  )
}
