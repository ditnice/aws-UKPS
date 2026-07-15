import { notFound } from 'next/navigation'

import { getOrganisationById } from '@/client/generated/sdk.gen'

interface Props {
  params: Promise<{ id: string }>
}

export default async function OrganisationPage({ params }: Props) {
  const { id } = await params
  const organisationId = Number(id)

  if (!Number.isInteger(organisationId)) {
    notFound()
  }

  const { data: organisation, error } = await getOrganisationById({
    path: { id: organisationId },
  })

  if (error || !organisation) {
    return (
      <section>
        <h1>Unable to load organisation</h1>
        <p role="alert">There was a problem retrieving the organisation. Please try again later.</p>
      </section>
    )
  }

  return (
    <section>
      <h1>{organisation.organisationName}</h1>
      <dl>
        <dt>Organisation type</dt>
        <dd>{organisation.organisationType}</dd>

        <dt>Status</dt>
        <dd>{organisation.status}</dd>

        <dt>Head office email</dt>
        <dd>{organisation.headOfficeEmail}</dd>
      </dl>
    </section>
  )
}
