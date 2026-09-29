import { notFound } from 'next/navigation'

import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import RemoveUserControls from './RemoveUserControls'

interface Props {
  params: Promise<{ id: string; userId: string }>
}

export default async function RemoveUser({ params }: Props) {
  const { id, userId } = await params
  const organisationId = Number(id)
  const selectedUserId = Number(userId)

  if (!Number.isInteger(organisationId) || !Number.isInteger(selectedUserId)) {
    notFound()
  }

  const apiClient = await createServerApiClient()
  const { data: user, response } = await getUserDetailsWithinOrganisation({
    client: apiClient,
    path: { userId: selectedUserId, organisationId },
  })

  if (response?.status === 404) {
    notFound()
  }

  const backLink = (
    <BackLink href={`/portal/organisations/${organisationId}/manage-user-access/${userId}`}>
      Back
    </BackLink>
  )

  if (!user) {
    return (
      <>
        <PageHeader backLink={backLink} heading="Remove user" />
        <p role="alert">There was a problem retrieving the user. Please try again later.</p>
      </>
    )
  }

  return (
    <>
      <PageHeader backLink={backLink} heading="Remove user" />
      <p>You are about to remove {user.workEmail} from your organisation.</p>
      <p>
        This will permanently remove their personal information from UK PharmaScan. Any records
        created by them will not be affected.
      </p>

      <RemoveUserControls organisationId={organisationId} userId={selectedUserId} />
    </>
  )
}
