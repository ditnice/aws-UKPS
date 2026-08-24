import Link from 'next/link'
import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

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
      <p>You are about to approve julie.brooks@email.com&#39;s request for an account.</p>
      <p>Once approved they will be able to access your organisation&#39;s UKPS account.</p>

      <ButtonGroup>
        <Button variant="cta">Approve user</Button>
        <Button elementType={Link} href={organisationHref} variant="secondary">
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}
