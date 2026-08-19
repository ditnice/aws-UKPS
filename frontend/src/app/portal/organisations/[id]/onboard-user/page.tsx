import { notFound } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { OrganisationOnboardUserForm } from './_components/OrganisationOnboardUserForm'

interface Props {
  params: Promise<{ id: string }>
}

export default async function OrganisationOnboardUserPage({ params }: Props) {
  const { id } = await params
  const organisationId = Number(id)

  if (!Number.isInteger(organisationId)) {
    notFound()
  }

  return (
    <>
      <PageHeader
        backLink={<BackLink href={`/portal/organisations/${organisationId}`}>Back</BackLink>}
        heading="Add a new user"
      ></PageHeader>

      <OrganisationOnboardUserForm organisationId={organisationId} />
    </>
  )
}
