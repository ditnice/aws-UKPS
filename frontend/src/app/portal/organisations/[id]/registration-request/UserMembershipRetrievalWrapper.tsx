import { notFound } from 'next/navigation'

import {
  getOrganisationsByOrganisationIdUsersByUserIdMembershipRequests,
  UserMembershipRequestDto,
} from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'

export type UserMembershipRetrievalWrapperProps = {
  organisationId: number
  userId: number
  children: (request: UserMembershipRequestDto) => React.ReactNode
}
const UserMembershipRetrievalWrapper = async ({
  organisationId,
  userId,
  children,
}: UserMembershipRetrievalWrapperProps) => {
  const client = await createServerApiClient()
  const { data, error } = await getOrganisationsByOrganisationIdUsersByUserIdMembershipRequests({
    client,
    path: { organisationId, userId },
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
