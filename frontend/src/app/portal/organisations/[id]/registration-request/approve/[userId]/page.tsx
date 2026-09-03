import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import ModifyUserMembershipRequestControls from '../../ModifyUserMembershipRequestControls'
import UserMembershipRetrievalWrapper from '../../UserMembershipRetrievalWrapper'

interface Props {
  params: Promise<{ id: string; userId: string }>
}

export default async function ApproveUser({ params }: Props) {
  const { id, userId } = await params
  const organisationId = Number(id)
  const selectedUserId = Number(userId)

  if (!Number.isInteger(organisationId) || !Number.isInteger(selectedUserId)) {
    notFound()
  }

  const organisationHref = `/portal/organisations/${organisationId}`

  return (
    <>
      <PageHeader
        backLink={<BackLink href={organisationHref}>Back</BackLink>}
        heading="Approve user"
      />
      <UserMembershipRetrievalWrapper organisationId={organisationId} userId={selectedUserId}>
        {(request) => (
          <>
            <p>You are about to approve {request.workEmail}&#39;s request for an account.</p>
            <p>Once approved they will be able to access your organisation&#39;s UKPS account.</p>

            <ModifyUserMembershipRequestControls
              action="Approve"
              organisationId={organisationId}
              userId={selectedUserId}
              successLink={`${organisationHref}?invited=${encodeURIComponent(request.workEmail)}`}
              backLink={organisationHref}
            />
          </>
        )}
      </UserMembershipRetrievalWrapper>
    </>
  )
}
