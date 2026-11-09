import { notFound } from 'next/navigation'

import { getOrganisationById, getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

import { NotPermitted } from '../_components/NotPermitted'

import { OrganisationOnboardUserForm } from './_components/OrganisationOnboardUserForm'

interface Props {
  params: Promise<{ id: string }>
}

export default async function OrganisationOnboardUserPage({ params }: Props) {
  const { id } = await params
  const organisationId = parsePositiveInteger(id)

  if (!organisationId) {
    notFound()
  }

  const client = await createServerApiClient()
  const { data: currentUser, error: currentUserError } = await getUsersMe({ client })

  if (currentUserError || !currentUser) {
    return (
      <section>
        <PageHeader heading="Unable to load current user" />
        <p role="alert">There was a problem retrieving the current user. Please try again later.</p>
      </section>
    )
  }

  // Only champion and super users can invite users, so a standard user can't use the form.
  if (currentUser.userRole === 'Standard') {
    return <NotPermitted />
  }

  // Resolve access through the backend before offering a form for this organisation. The server
  // client handles organisation-selection redirects centrally (ADR-002/003).
  const { data: organisation, error } = await getOrganisationById({
    client,
    path: { id: organisationId },
  })
  if (error || !organisation) {
    return (
      <section>
        <PageHeader heading="Unable to load organisation" />
        <ErrorState>
          There was a problem retrieving the organisation. Please try again later.
        </ErrorState>
      </section>
    )
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
