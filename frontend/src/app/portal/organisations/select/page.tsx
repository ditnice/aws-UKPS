import { redirect } from 'next/navigation'

import { getCurrentUserOrganisations } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { SelectOrganisationTable } from './_components/SelectOrganisationTable'

export const dynamic = 'force-dynamic'

const heading = 'Select the organisation to manage'

const SelectOrganisationPage = async () => {
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
    redirect(`/portal/organisations/${organisations[0].id}/records`)
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
      <SelectOrganisationTable organisations={organisations} />
    </>
  )
}

export default SelectOrganisationPage
