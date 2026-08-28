import { notFound } from 'next/navigation'

import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { isSwitchableRole, roleDescriptions } from '../_lib/userRoles'

import { ChangePermissionsForm } from './_components/ChangePermissionsForm'

interface Props {
  params: Promise<{ id: string; userId: string }>
}

const championCapabilities = [
  'adding users to your organisation',
  'changing user roles',
  'deactivating and removing users',
]

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

  const currentRole = user.userRole

  return (
    <>
      <PageHeader backLink={backLink} heading="Change user permissions" />
      <p>
        {user.workEmail} is {roleDescriptions[currentRole]}.
      </p>

      {isSwitchableRole(currentRole) ? (
        <>
          <p>
            {currentRole === 'Standard'
              ? 'If you change this user’s role, they will gain access to additional capabilities in UK PharmaScan, including:'
              : 'If you change this user’s role, they will lose access to the following capabilities in UK PharmaScan:'}
          </p>
          <ul>
            {championCapabilities.map((capability) => (
              <li key={capability}>{capability}</li>
            ))}
          </ul>

          <ChangePermissionsForm
            currentRole={currentRole}
            membershipId={Number(user.organisationMembershipId)}
            organisationId={organisationId}
            userId={selectedUserId}
          />
        </>
      ) : (
        <p>This user’s role cannot be changed from here.</p>
      )}
    </>
  )
}
