import Link from 'next/link'
import { notFound } from 'next/navigation'
import { Suspense } from 'react'

import { FilterSummary } from '@nice-digital/nds-filters'
import { Grid, GridItem } from '@nice-digital/nds-grid'

import { getOrganisationById } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'

import { OrganisationActionAlert } from './_components/OrganisationActionAlert'
import { OrganisationFilters } from './_components/OrganisationFilters'
import { OrganisationUsersTable } from './_components/OrganisationUsersTable'
import { UserActionAlert } from './_components/UserActionAlert'
import { parseOrganisationAction } from './_lib/organisationActionsAlert'
import { parseUserAction, type UserActionSearchParams } from './_lib/userActionAlert'
import {
  buildUserListHref,
  getActiveFilters,
  parseUserListQuery,
  UserListQuery,
  type UserListSearchParams,
} from './_lib/userListQuery'

interface Props {
  params: Promise<{ id: string }>
  searchParams: Promise<UserListSearchParams & UserActionSearchParams>
}

export default async function OrganisationPage({ params, searchParams }: Props) {
  const { id } = await params
  const resolvedSearchParams = await searchParams
  const query = parseUserListQuery(resolvedSearchParams)
  const activeFilters = getActiveFilters(query)
  const userAction = parseUserAction(resolvedSearchParams)
  const organisationAction = parseOrganisationAction(resolvedSearchParams)
  const organisationId = Number(id)

  if (!Number.isInteger(organisationId)) {
    notFound()
  }

  const apiClient = await createServerApiClient()
  const { data: organisation, error } = await getOrganisationById({
    client: apiClient,
    path: { id: organisationId },
  })

  if (error || !organisation) {
    return (
      <section>
        <PageHeader heading="Unable to load organisation" />
        <p role="alert">There was a problem retrieving the organisation. Please try again later.</p>
      </section>
    )
  }

  return (
    <>
      {userAction && (
        <UserActionAlert
          apiClient={apiClient}
          organisationId={organisationId}
          userAction={userAction}
        />
      )}
      {organisationAction && (
        <OrganisationActionAlert organisationAction={organisationAction.action} />
      )}

      <PageHeader
        heading={organisation.organisationName}
        backLink={<BackLink href={'/portal'}>Back</BackLink>}
      />

      <h2>Organisation details</h2>
      <SummaryList variant="two-column">
        <SummaryListRow label="Organisation type" value={organisation.organisationType} />
        <SummaryListRow label="Organisation name" value={organisation.organisationName} />
        <SummaryListRow label="Head office address" value={organisation.headOfficeAddress} />
        <SummaryListRow label="Head office email address" value={organisation.headOfficeEmail} />
        <SummaryListRow label="Head office phone number" value={organisation.headOfficeTelephone} />
      </SummaryList>

      <Button variant="secondary" to={`/portal/organisations/${organisationId}/edit`}>
        Edit details
      </Button>

      <h2>Search and filter</h2>
      <Grid gutter="loose">
        <GridItem cols={12} md={4} lg={3} elementType="section" aria-label="Filter results">
          <OrganisationFilters query={query} />
        </GridItem>
        <GridItem cols={12} md={8} lg={9} elementType="section" aria-labelledby="filter-summary">
          <Suspense fallback={<p>Loading users...</p>} key={buildUserListHref(query)}>
            <OrganisationUsersTable
              apiClient={apiClient}
              organisationId={organisationId}
              query={query}
            />
          </Suspense>
        </GridItem>
      </Grid>
    </>
  )
}
