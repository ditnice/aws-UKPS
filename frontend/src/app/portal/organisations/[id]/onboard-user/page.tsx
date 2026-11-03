import { notFound } from 'next/navigation'

import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { NotPermitted } from '../_components/NotPermitted'

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

  const { data: currentUser, error: currentUserError } = await getUsersMe({
    client: await createServerApiClient(),
  })

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
