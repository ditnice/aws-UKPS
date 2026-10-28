import Link from 'next/link'
import { notFound, redirect, RedirectType } from 'next/navigation'

import { getCurrentUserOrganisations, getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { getOrganisationLandingHref, selectOrganisationPath } from '@/lib/auth/routing'
import { parsePositiveInteger } from '@/lib/valueParsing'

import { SwitchOrganisationControls } from './_components/SwitchOrganisationControls'

export const dynamic = 'force-dynamic'

const heading = 'Switch organisation'

type SwitchOrganisationPageProps = {
  searchParams: Promise<{ organisationId?: string; returnTo?: string }>
}

const SwitchOrganisationPage = async ({ searchParams }: SwitchOrganisationPageProps) => {
  const { organisationId: organisationIdParam, returnTo } = await searchParams
  const organisationId = parsePositiveInteger(organisationIdParam ?? '')

  if (!organisationId) {
    notFound()
  }

  const client = await createServerApiClient()
  const [{ data: currentUser }, { data: organisations }] = await Promise.all([
    getUsersMe({ client }),
    getCurrentUserOrganisations({ client }),
  ])

  if (!currentUser || !organisations) {
    return (
      <>
        <PageHeader heading={heading} />
        <ErrorState data-testid="organisation-retrieval-error">
          An error occurred when retrieving your organisations. Please try again later.
        </ErrorState>
      </>
    )
  }

  // Another tab or device may have already switched to the organisation.
  if (currentUser.organisationId === organisationId) {
    redirect(getOrganisationLandingHref(organisationId, returnTo), RedirectType.replace)
  }

  const organisation = organisations.find((x) => x.id === organisationId)

  if (!organisation) {
    return (
      <>
        <PageHeader heading={heading} />
        <ErrorState data-testid="organisation-not-available">
          You do not have access to manage this organisation.
        </ErrorState>
        <p>
          <Link href={selectOrganisationPath}>Choose an organisation to manage</Link>
        </p>
      </>
    )
  }

  return (
    <>
      <PageHeader heading={heading} />
      <p>
        You are currently signed in for {currentUser.organisationName}. Do you wish to switch to{' '}
        {organisation.organisationName}?
      </p>
      <SwitchOrganisationControls
        organisationId={organisationId}
        confirmHref={getOrganisationLandingHref(organisationId, returnTo)}
        cancelHref={`/portal/organisations/${currentUser.organisationId}/records`}
      />
    </>
  )
}

export default SwitchOrganisationPage
