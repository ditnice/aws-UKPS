import { notFound } from 'next/navigation'

import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { ChangePermissionsForm } from './_components/ChangePermissionsForm'

interface Props {
  params: Promise<{ id: string; userId: string }>
}

export default async function ChangeUserPermissions({ params }: Props) {
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
    <BackLink href={`/portal/organisations/${organisationId}/manage-user-access/${selectedUserId}`}>
      Back
    </BackLink>
  )

  if (!user) {
    return (
      <>
        <PageHeader backLink={backLink} heading="Change user permissions" />
        <p role="alert">There was a problem retrieving the user. Please try again later.</p>
      </>
    )
  }

  return (
    <>
      <PageHeader backLink={backLink} heading="Change user permissions" />
      <p>
        {user.workEmail} is a {user.userRole.toLowerCase()} user.
      </p>

      <>
        <p>
          If you change this user’s role, they will{' '}
          {user.userRole === 'Standard'
            ? 'gain access to additional capabilities in UK PharmaScan, including:'
            : 'lose access to the following capabilities in UK PharmaScan:'}
        </p>
        <ul>
          <li>adding users to your organisation</li>
          <li>changing user roles</li>
          <li>deactivating and removing users</li>
        </ul>

        <ChangePermissionsForm
          currentRole={user.userRole}
          membershipId={Number(user.organisationMembershipId)}
          organisationId={organisationId}
          userId={selectedUserId}
        />
      </>
    </>
  )
}
