import { notFound } from 'next/navigation'

import { getUserDetailsWithinOrganisation } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import ReactivateUserControls from './ReactivateUserControls'

type ReactivateUserPageProps = {
  params: Promise<{ id: string; userId: string }>
}
export default async function ReactivateUserPage({ params }: ReactivateUserPageProps) {
  const { id, userId } = await params

  const organisationId = Number(id)
  const selectedUserId = Number(userId)

  if (!Number.isInteger(organisationId) || !Number.isInteger(selectedUserId)) {
    notFound()
  }

  return (
    <>
      <PageHeader heading="Reactivate user" backLink={<BackLinkBrowser />} />
      <PageContent organisationId={organisationId} userId={selectedUserId} />
    </>
  )
}

const PageContent = async ({
  organisationId,
  userId,
}: {
  organisationId: number
  userId: number
}) => {
  const client = await createServerApiClient()
  const { data: user, error } = await getUserDetailsWithinOrganisation({
    client,
    path: { organisationId, userId },
  })

  if (!user || error)
    return <ErrorState>An error occurred when trying to retrieve the user.</ErrorState>

  return (
    <>
      <p>You are about to reactivate {user.workEmail}.</p>
      <p>They will regain access to UK PharmaScan.</p>
      <ReactivateUserControls
        organisationId={organisationId}
        userId={userId}
        membershipId={user.organisationMembershipId}
      />
    </>
  )
}
