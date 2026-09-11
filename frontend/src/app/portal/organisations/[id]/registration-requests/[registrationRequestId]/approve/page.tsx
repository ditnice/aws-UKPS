import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import ModifyUserMembershipRequestControls from '../ModifyUserMembershipRequestControls'
import UserMembershipRetrievalWrapper from '../UserMembershipRetrievalWrapper'

interface Props {
  params: Promise<{ id: string; registrationRequestId: string }>
}

export default async function ApproveUser({ params }: Props) {
  const { id, registrationRequestId } = await params
  const organisationId = Number(id)
  const parsedRegistrationRequestId = Number(registrationRequestId)

  if (!Number.isInteger(organisationId) || !Number.isInteger(parsedRegistrationRequestId)) {
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
        registrationRequestId={parsedRegistrationRequestId}
      >
        {(request) => (
          <>
            <p>You are about to approve {request.workEmail}&#39;s request for an account.</p>
            <p>Once approved they will be able to access your organisation&#39;s UKPS account.</p>

            <ModifyUserMembershipRequestControls
              action="Approve"
              organisationId={organisationId}
              registrationRequestId={parsedRegistrationRequestId}
              successLink={`${organisationHref}?invited=${encodeURIComponent(request.workEmail)}`}
              backLink={organisationHref}
            />
          </>
        )}
      </UserMembershipRetrievalWrapper>
    </>
  )
}
