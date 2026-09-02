import { notFound } from 'next/navigation'

import { getOrganisationsByOrganisationIdUsersByUserIdMembershipRequests } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { errorMessages } from '@/lib/form/errorMessages'

import ModifyUserMembershipRequestControls from '../../ModifyUserMembershipRequestControls'
import ModifyUserMembershipRequestControls from '../../ModifyUserMembershipRequestControls'
import UserMembershipRetrievalWrapper from '../../UserMembershipRetrievalWrapper'

interface Props {
  params: Promise<{ id: string; userId: string }>
}

export default async function RejectUser({ params }: Props) {
  const { id, userId } = await params
  const organisationId = Number(id)
  const selectedUserId = Number(userId)

  if (!Number.isInteger(organisationId) || !Number.isInteger(selectedUserId)) {
    notFound()
  }

  const exampleUserEmail = 'julie.brooks@email.com'

  const organisationHref = `/portal/organisations/${organisationId}`
  return (
    <>
      <PageHeader
        backLink={<BackLink href={organisationHref}>Back</BackLink>}
        heading="Reject user"
      />
      <UserMembershipRetrievalWrapper organisationId={organisationId} userId={selectedUserId}>
        {(request) => (
          <>
            <p>You are about to reject {request.workEmail}&#39;s request for an account.</p>

            <ModifyUserMembershipRequestControls
              action="Reject"
              organisationId={organisationId}
              userId={selectedUserId}
              successLink={`${organisationHref}`}
              backLink={organisationHref}
            />
          </>
        )}
      </UserMembershipRetrievalWrapper>
    </>
  )
}
