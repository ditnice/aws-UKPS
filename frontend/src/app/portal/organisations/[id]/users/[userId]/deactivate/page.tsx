import { notFound } from 'next/navigation'

import { getUserDetailsWithinOrganisation } from '@/client/generated'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import DeactivateUserControls from './DeactivateUserControls'

type DeactivateUserPageProps = {
  params: Promise<{ id: string; userId: string }>
}
export default async function DeactivateUserPage({ params }: DeactivateUserPageProps) {
  const { id, userId } = await params

  const organisationId = Number(id)
  const selectedUserId = Number(userId)

  if (!Number.isInteger(organisationId) || !Number.isInteger(selectedUserId)) {
    notFound()
  }

  const { data: user, error } = await getUserDetailsWithinOrganisation({
    path: { organisationId, userId: selectedUserId },
  })

  const renderPageContent = () => {
    if (!user || error)
      return <ErrorState>An error occurred when trying to retrieve the user.</ErrorState>

    return (
      <>
        <p>You are about to about to deactivate {user.workEmail}</p>
        <p>
          A deactivated user will remain on UK PharmaScan but will not receive any communications
          until they are reactivated.
        </p>
        <DeactivateUserControls
          organisationId={organisationId}
          membershipId={user.organisationMembershipId}
        />
      </>
    )
  }

  return (
    <>
      <PageHeader heading="Deactivate user" backLink={<BackLinkBrowser />} />
      {renderPageContent()}
    </>
  )
}
