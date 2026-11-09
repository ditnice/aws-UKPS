import { notFound } from 'next/navigation'

import { getOrganisationById, getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { NotPermitted } from '../_components/NotPermitted'

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
  const [{ data: currentUser, error: currentUserError }, { data: organisation, error }] =
    await Promise.all([
      getUsersMe({ client: apiClient }),
      getOrganisationById({ client: apiClient, path: { id: organisationId } }),
    ])

  if (currentUserError || !currentUser) {
    return (
      <section>
        <PageHeader heading="Unable to load current user" />
        <p role="alert">There was a problem retrieving the current user. Please try again later.</p>
      </section>
    )
  }

  // Only champion and super users can update an organisation, so a standard user can't use the form.
  if (currentUser.userRole === 'Standard') {
    return <NotPermitted />
  }

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
        heading="Edit your organisation's details"
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
