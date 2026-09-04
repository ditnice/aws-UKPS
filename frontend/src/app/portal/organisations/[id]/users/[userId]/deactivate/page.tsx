import { notFound } from 'next/navigation'

import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'

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

  const placeholderEmail = 'julie.brooks@example.com'
  const placeholderMembershipId = 1

  const renderPageContent = () => {
    return (
      <>
        <p>You are about to about to deactivate {placeholderEmail}</p>
        <p>
          A deactivated user will remain on UK PharmaScan but will not receive any communications
          until they are reactivated.
        </p>
        <DeactivateUserControls
          organisationId={organisationId}
          membershipId={placeholderMembershipId}
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
