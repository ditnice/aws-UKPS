import { getOrganisations } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { Table } from '@/components/Table/Table'

import ManageOrganisationLink from './ManageOrganisationLink'

const SelectOrganisationPage = async ({
  searchParams,
}: {
  searchParams: Promise<{ error: string }>
}) => {
  const { data: organisations, error } = await getOrganisations({
    client: await createServerApiClient(),
  })
  const { error: organisationSelectionError } = await searchParams

  if (!organisations || error) {
    return (
      <>
        <PageHeader heading="Failed to retrieve organisations" />
      </>
    )
  }

  return (
    <>
      <PageHeader heading="Select the organisation to manage" />
      <p>
        Your email address is associated with multiple organisations. Choose the organisation you
        want to manage.
      </p>
      {organisationSelectionError && (
        <ErrorState>
          An error occurred when attempting to set the organisation you are managing
        </ErrorState>
      )}
      <Table columnWidth="content">
        <thead>
          <tr>
            <th scope="col">Organisation</th>
            <th scope="col">Action</th>
          </tr>
        </thead>
        <tbody>
          {organisations.map((x) => (
            <tr key={x.id}>
              <td>{x.organisationName}</td>
              <td>
                <ManageOrganisationLink organisationId={x.id} />
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
    </>
  )
}

export default SelectOrganisationPage
