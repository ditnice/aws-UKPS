import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import ModifyUserMembershipRequestControls from '../ModifyUserMembershipRequestControls'
import UserMembershipRetrievalWrapper from '../UserMembershipRetrievalWrapper'

interface Props {
  params: Promise<{ id: string; registrationRequestId: string }>
}

export default async function RejectUser({ params }: Props) {
  const { id, registrationRequestId } = await params
  const organisationId = Number(id)
  const parsedRegistrationRequestId = Number(registrationRequestId)

  if (!Number.isInteger(organisationId) || !Number.isInteger(parsedRegistrationRequestId)) {
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
        registrationRequestId={parsedRegistrationRequestId}
      >
        {(request) => (
          <>
            <p>You are about to reject {request.workEmail}&#39;s request for an account.</p>

            <ModifyUserMembershipRequestControls
              action="Reject"
              organisationId={organisationId}
              registrationRequestId={parsedRegistrationRequestId}
              successLink={`${organisationHref}`}
              backLink={organisationHref}
            />
          </>
        )}
      </UserMembershipRetrievalWrapper>
    </>
  )
}
