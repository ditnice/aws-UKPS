import Link from 'next/link'
import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

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

  const organisationHref = `/portal/organisations/${organisationId}`

  return (
    <>
      <PageHeader
        backLink={<BackLink href={organisationHref}>Back</BackLink>}
        heading="Reject user"
      />
      <p>You are about to reject julie.brooks@email.com&#39;s request for an account.</p>

      <ButtonGroup>
        <Button variant="cta">Reject user</Button>
        <Button elementType={Link} href={organisationHref} variant="secondary">
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}
