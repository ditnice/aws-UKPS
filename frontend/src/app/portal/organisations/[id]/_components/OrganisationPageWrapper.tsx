import { notFound } from 'next/navigation'

import { getOrganisationById, OrganisationDetailsDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

/**
 * Props for the {@link OrganisationPageWrapper} component.
 */
export type OrganisationPageWrapperProps = {
  /**
   * The organisation identifier from the route.
   */
  organisationId: string

  /**
   * Renders the page content using the retrieved organisation.
   */
  children: (org: OrganisationDetailsDto) => React.ReactNode
}

/**
 * Server-side wrapper that retrieves an organisation and provides it to
 * the page content.
 *
 * The organisation ID is validated as an integer before the organisation is
 * retrieved. An invalid ID results in a not-found response. If the
 * organisation cannot be retrieved, an error message is rendered instead.
 *
 * @param props - The component props.
 * @returns The rendered page content, or an error message when the organisation
 * cannot be retrieved.
 */
const OrganisationPageWrapper = async ({
  organisationId,
  children,
}: OrganisationPageWrapperProps) => {
  const organisationIdNumber = parsePositiveInteger(organisationId)

  if (!organisationIdNumber) {
    notFound()
  }

  const apiClient = await createServerApiClient()
  const { data: organisation, error } = await getOrganisationById({
    client: apiClient,
    path: { id: organisationIdNumber },
  })

  if (error || !organisation) {
    return (
      <section>
        <PageHeader heading="Unable to load organisation" />
        <ErrorState data-testid="organisation-retrieval-error">
          There was a problem retrieving the organisation. Please try again later.
        </ErrorState>
      </section>
    )
  }

  return <>{children(organisation)}</>
}

export default OrganisationPageWrapper
