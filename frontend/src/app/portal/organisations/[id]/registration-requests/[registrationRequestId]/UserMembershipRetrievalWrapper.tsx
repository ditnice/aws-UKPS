import { notFound } from 'next/navigation'

import { getUserRegistrationById, RegisterUserConfirmationDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'

export type UserMembershipRetrievalWrapperProps = {
  organisationId: number
  requestGuid: string
  children: (request: RegisterUserConfirmationDto) => React.ReactNode
}
const UserMembershipRetrievalWrapper = async ({
  organisationId,
  requestGuid,
  children,
}: UserMembershipRetrievalWrapperProps) => {
  const client = await createServerApiClient()
  const { data, error } = await getUserRegistrationById({
    client,
    path: { organisationId, requestGuid },
  })
  if (error?.status == 404) {
    notFound()
  }

  if (!data || error) {
    return (
      <p role="alert" data-testid="failure-message">
        {errorMessages.anErrorOccurredWhenTryingToRetrieveTheUserMembershipRequest}
      </p>
    )
  }
  return children(data)
}

export default UserMembershipRetrievalWrapper
