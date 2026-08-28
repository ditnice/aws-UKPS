import Link from 'next/link'
import { notFound } from 'next/navigation'

import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { isSwitchableRole, roleDescriptions } from './_lib/userRoles'

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

  console.log({ user, response })

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
        {user.workEmail} is {roleDescriptions[user.userRole]}.
      </p>
      <p>Choose what you want to do:</p>
      <ul>
        <li>Change permissions - change what the user can do.</li>
        <li>
          Deactivate user - temporarily stop the user&#39;s access. You can reactivate them later.
        </li>
        <li>Remove user - permanently remove the user&#39;s access.</li>
      </ul>

      <ButtonGroup>
        {isSwitchableRole(user.userRole) && (
          <Button
            elementType={Link}
            href={`/portal/organisations/${organisationId}/manage-user-access/${selectedUserId}/change-permissions`}
            variant="cta"
          >
            Change permissions
          </Button>
        )}
        <Button variant="secondary">Deactivate user</Button>
        <Button variant="secondary">Remove user</Button>
      </ButtonGroup>
    </>
  )
}
