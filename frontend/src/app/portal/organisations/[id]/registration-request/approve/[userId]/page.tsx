import Link from 'next/link'
import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import ModifyUserMembershipRequestControlsProps from '../../ModifyUserMembershipRequestControls'

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

  // TODO 313 - Implement method for retrieving requests for a user id and org id.
  const exampleUserEmail = 'julie.brooks@email.com'

  return (
    <>
      <PageHeader
        backLink={<BackLink href={organisationHref}>Back</BackLink>}
        heading="Approve user"
      />
      <p>You are about to approve {exampleUserEmail}&#39;s request for an account.</p>
      <p>Once approved they will be able to access your organisation&#39;s UKPS account.</p>

      <ModifyUserMembershipRequestControlsProps
        action="Approve"
        organisationId={organisationId}
        userId={selectedUserId}
        successLink={`${organisationHref}?invited=${encodeURIComponent(exampleUserEmail)}`}
        backLink={organisationHref}
      />
    </>
  )
}
