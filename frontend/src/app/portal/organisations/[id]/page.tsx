import { notFound } from 'next/navigation'
import { Suspense } from 'react'

import { Alert } from '@nice-digital/nds-alert'
import { Grid, GridItem } from '@nice-digital/nds-grid'

import {
  buildUserListHref,
  parseUserListQuery,
  type UserListSearchParams,
} from '@/app/portal/_utils/userListQuery'
import { getOrganisationById } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

import { OrganisationFilters } from './_components/OrganisationFilters'
import { OrganisationUsersTable } from './_components/OrganisationUsersTable'
import styles from './page.module.scss'

interface Props {
  params: Promise<{ id: string }>
  searchParams: Promise<UserListSearchParams & { invited?: string }>
}

export default async function OrganisationPage({ params, searchParams }: Props) {
  const { id } = await params
  const resolvedSearchParams = await searchParams
  const query = parseUserListQuery(resolvedSearchParams)
  const invitedEmail = resolvedSearchParams.invited?.trim()
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
      {invitedEmail && (
        <div className={styles.invitedAlert}>
          <Alert type="success">
            <h3>Invitation sent</h3>
            <p>
              We&rsquo;ve sent an email to {invitedEmail} with instructions to set up an account.
            </p>
          </Alert>
        </div>
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
          <OrganisationFilters />
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
