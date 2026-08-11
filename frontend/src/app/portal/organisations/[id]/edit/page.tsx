import { notFound } from 'next/navigation'

import { Breadcrumb, Breadcrumbs } from '@nice-digital/nds-breadcrumbs'

import { getOrganisationById } from '@/client/generated/sdk.gen'
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
    <>
      <Breadcrumbs>
        <Breadcrumb to="/portal">Dashboard</Breadcrumb>
        <Breadcrumb to={`/portal/organisations/${organisationId}`}>Manage organisation</Breadcrumb>
      </Breadcrumbs>

      <PageHeader heading="Edit your company's details" />

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
