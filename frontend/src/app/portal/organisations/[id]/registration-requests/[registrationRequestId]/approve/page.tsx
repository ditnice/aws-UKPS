import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { isRequestGuid } from '../../../_lib/requestGuid'
import { buildUserActionHref } from '../../../_lib/userActionAlert'
import ModifyUserMembershipRequestControls from '../ModifyUserMembershipRequestControls'
import UserMembershipRetrievalWrapper from '../UserMembershipRetrievalWrapper'

interface Props {
  params: Promise<{ id: string; registrationRequestId: string }>
}

export default async function ApproveUser({ params }: Props) {
  const { id, registrationRequestId } = await params
  const organisationId = Number(id)

  if (
    !Number.isInteger(organisationId) ||
    organisationId <= 0 ||
    !isRequestGuid(registrationRequestId)
  ) {
    notFound()
  }

  const organisationHref = `/portal/organisations/${organisationId}` as const

  return (
    <>
      <PageHeader
        backLink={<BackLink href={organisationHref}>Back</BackLink>}
        heading="Approve user"
      />
      <UserMembershipRetrievalWrapper
        organisationId={organisationId}
        requestGuid={registrationRequestId}
      >
        {(request) => (
          <>
            <p>You are about to approve {request.workEmail}&#39;s request for an account.</p>
            <p>Once approved they will be able to access your organisation&#39;s UKPS account.</p>

            <ModifyUserMembershipRequestControls
              action="Approve"
              organisationId={organisationId}
              requestGuid={registrationRequestId}
              successLink={buildUserActionHref(organisationId, {
                action: 'approved-request',
                userRequestId: registrationRequestId,
              })}
              backLink={organisationHref}
            />
          </>
        )}
      </UserMembershipRetrievalWrapper>
    </>
  )
}
