import { redirect, RedirectType } from 'next/navigation'

import { getCurrentUserOrganisations } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { getOrganisationLandingHref, getSafeReturnTo } from '@/lib/auth/routing'

import { SelectOrganisationTable } from './_components/SelectOrganisationTable'

export const dynamic = 'force-dynamic'

const heading = 'Select the organisation to manage'

type SelectOrganisationPageProps = {
  searchParams: Promise<{ returnTo?: string }>
}

const SelectOrganisationPage = async ({ searchParams }: SelectOrganisationPageProps) => {
  const returnTo = getSafeReturnTo((await searchParams).returnTo)
  const { data: organisations, error } = await getCurrentUserOrganisations({
    client: await createServerApiClient(),
  })

  if (!organisations || error) {
    return (
      <>
        <PageHeader heading={heading} />
        <ErrorState data-testid="organisation-retrieval-error">
          An error occurred when retrieving your organisations. Please try again later.
        </ErrorState>
      </>
    )
  }

  if (organisations.length === 1) {
    // Replace so the selection page is not left in the browser history for the back button.
    redirect(getOrganisationLandingHref(organisations[0].id, returnTo), RedirectType.replace)
  }

  if (organisations.length === 0) {
    return (
      <>
        <PageHeader heading={heading} />
        <p data-testid="no-organisations">
          You do not currently have access to manage any organisations.
        </p>
      </>
    )
  }

  return (
    <>
      <PageHeader heading={heading} />
      <p>
        Your email address is associated with multiple organisations. Choose the organisation you
        want to manage.
      </p>
      <SelectOrganisationTable organisations={organisations} returnTo={returnTo} />
    </>
  )
}

export default SelectOrganisationPage
