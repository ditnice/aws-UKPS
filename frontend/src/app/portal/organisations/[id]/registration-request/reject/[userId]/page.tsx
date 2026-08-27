import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import ModifyUserMembershipRequestControls from '../../ModifyUserMembershipRequestControls'

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
      <p>You are about to reject {exampleUserEmail}&#39;s request for an account.</p>

      <ModifyUserMembershipRequestControls
        action="Reject"
        organisationId={organisationId}
        userId={selectedUserId}
        successLink={`${organisationHref}`}
        backLink={organisationHref}
      />
    </>
  )
}
