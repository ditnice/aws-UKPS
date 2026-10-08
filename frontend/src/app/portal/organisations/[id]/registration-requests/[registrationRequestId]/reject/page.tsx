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

export default async function RejectUser({ params }: Props) {
  const { id, registrationRequestId } = await params
  const organisationId = Number(id)

  if (
    !Number.isInteger(organisationId) ||
    organisationId <= 0 ||
    !isRequestGuid(registrationRequestId)
  ) {
    notFound()
  }

  const organisationHref = `/portal/organisations/${organisationId}`
  return (
    <>
      <PageHeader
        backLink={<BackLink href={organisationHref}>Back</BackLink>}
        heading="Reject user"
      />
      <UserMembershipRetrievalWrapper
        organisationId={organisationId}
        requestGuid={registrationRequestId}
      >
        {(request) => (
          <>
            <p>You are about to reject {request.workEmail}&#39;s request for an account.</p>

            <ModifyUserMembershipRequestControls
              action="Reject"
              organisationId={organisationId}
              requestGuid={registrationRequestId}
              successLink={buildUserActionHref(organisationId, {
                action: 'rejected-request',
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
