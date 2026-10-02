import { notFound } from 'next/navigation'

import { getUserDetailsWithinOrganisation, getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import ManageUserAccessActions from './_components/ManageUserAccessActions'

interface Props {
  params: Promise<{ id: string; userId: string }>
}

export default async function ManageUserAccess({ params }: Props) {
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

  const { data: currentUser } = await getUsersMe({
    client: apiClient,
  })

  if (!currentUser) {
    notFound()
  }

  if (response?.status === 404) {
    notFound()
  }
  const backLink = <BackLink href={`/portal/organisations/${organisationId}`}>Back</BackLink>

  if (!user) {
    return (
      <>
        <PageHeader backLink={backLink} heading="Manage user's access" />
        <p role="alert">There was a problem retrieving the user. Please try again later.</p>
      </>
    )
  }

  return (
    <>
      <PageHeader backLink={backLink} heading="Manage user&#39;s access" />
      <p>
        {user.workEmail} is a {user.userRole.toLowerCase()} user.
      </p>
      <ManageUserAccessActions
        organisationId={organisationId}
        selectedUserId={selectedUserId}
        currentUserRole={currentUser.userRole}
      />
    </>
  )
}
