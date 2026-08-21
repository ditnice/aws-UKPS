import { notFound } from 'next/navigation'

import { getOrganisationById } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { EditOrganisationDetailsForm } from './_components/EditOrganisationDetailsForm'

interface Props {
  params: Promise<{ id: string }>
}

export default async function EditOrganisationPage({ params }: Props) {
  const { id } = await params
  const organisationId = Number(id)

  if (!Number.isInteger(organisationId)) {
    notFound()
  }

  const apiClient = await createServerApiClient()
  const { data: organisation, error } = await getOrganisationById({
    client: apiClient,
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
    <>
      <PageHeader
        backLink={<BackLink href={`/portal/organisations/${organisationId}`}>Back</BackLink>}
        heading="Edit your company's details"
      ></PageHeader>

      <EditOrganisationDetailsForm
        organisationId={organisationId}
        organisationName={organisation.organisationName}
        headOfficeAddress={organisation.headOfficeAddress}
        headOfficeEmail={organisation.headOfficeEmail}
        headOfficeTelephone={organisation.headOfficeTelephone}
      />
    </>
  )
}
