import { notFound } from 'next/navigation'
import { Suspense } from 'react'

import { Button } from '@nice-digital/nds-button'
import { Grid, GridItem } from '@nice-digital/nds-grid'
import { PageHeader } from '@nice-digital/nds-page-header'

import { getOrganisationById } from '@/client/generated/sdk.gen'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

import { OrganisationFilters } from './_components/OrganisationFilters'
import { OrganisationUsersTable } from './_components/OrganisationUsersTable'

const pageSizeOptions = [10, 25, 50]
const defaultPageSize = 10

interface Props {
  params: Promise<{ id: string }>
  searchParams: Promise<{ page?: string; pageSize?: string }>
}

function parsePage(page: string | undefined): number {
  const parsedPage = Number(page)

  return Number.isInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1
}

function parsePageSize(pageSize: string | undefined): number {
  const parsedPageSize = Number(pageSize)

  return pageSizeOptions.includes(parsedPageSize) ? parsedPageSize : defaultPageSize
}

export default async function OrganisationPage({ params, searchParams }: Props) {
  const { id } = await params
  const { page, pageSize: pageSizeParam } = await searchParams
  const organisationId = Number(id)
  const currentPage = parsePage(page)
  const pageSize = parsePageSize(pageSizeParam)

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
      <PageHeader heading={organisation.organisationName} />

      <h2>Organisation details</h2>
      <SummaryList variant="two-column">
        <SummaryListRow label="Organisation type" value={organisation.organisationType} />
        <SummaryListRow label="Organisation name" value={organisation.organisationName} />
        <SummaryListRow label="Head office address" value={organisation.headOfficeAddress} />
        <SummaryListRow
          label="Head office email address"
          value={organisation.headOfficeEmail}
        ></SummaryListRow>
        <SummaryListRow
          label="Head office phone number"
          value={organisation.headOfficeTelephone}
        ></SummaryListRow>
      </SummaryList>

      <Button variant={Button.variants.secondary}>Edit details</Button>

      <h2>Search and filter</h2>
      <Grid gutter="loose">
        <GridItem cols={12} md={4} lg={3} elementType="section" aria-label="Filter results">
          <OrganisationFilters />
        </GridItem>
        <GridItem cols={12} md={8} lg={9} elementType="section" aria-labelledby="filter-summary">
          <Suspense fallback={<p>Loading users...</p>} key={`${currentPage}-${pageSize}`}>
            <OrganisationUsersTable
              currentPage={currentPage}
              organisationId={organisationId}
              pageSize={pageSize}
            />
          </Suspense>
        </GridItem>
      </Grid>
    </>
  )
}
