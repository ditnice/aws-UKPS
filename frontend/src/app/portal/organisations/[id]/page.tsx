import { Suspense } from 'react'

import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

import { TableAndFiltersGrid } from '../../components/_components/TableAndFiltersGrid'

import { OrganisationActionAlert } from './_components/OrganisationActionAlert'
import { OrganisationFilters } from './_components/OrganisationFilters'
import OrganisationPageWrapper from './_components/OrganisationPageWrapper'
import { OrganisationUsersTable } from './_components/OrganisationUsersTable'
import { UserActionAlert } from './_components/UserActionAlert'
import { parseOrganisationAction } from './_lib/organisationActionsAlert'
import { parseUserAction, type UserActionSearchParams } from './_lib/userActionAlert'
import {
  buildUserListHref,
  parseUserListQuery,
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
  const userAction = parseUserAction(resolvedSearchParams)
  const organisationAction = parseOrganisationAction(resolvedSearchParams)
  const apiClient = await createServerApiClient()
  const { data: currentUser, error: currentUserError } = await getUsersMe({ client: apiClient })

  if (currentUserError || !currentUser) {
    return (
      <section>
        <PageHeader heading="Unable to load current user" />
        <p role="alert">There was a problem retrieving the current user. Please try again later.</p>
      </section>
    )
  }

  if (currentUser.userRole == 'Standard') {
    return (
      <section>
        <PageHeader heading="User is not permitted to view this page" />
        <p role="alert">
          The current user is not permitted to view this page. Please contact your champion user.
        </p>
      </section>
    )
  }

  return (
    <OrganisationPageWrapper organisationId={id}>
      {(organisation) => (
        <>
          {userAction && (
            <UserActionAlert organisationId={organisation.id} userAction={userAction} />
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
            <SummaryListRow
              label="Head office email address"
              value={organisation.headOfficeEmail}
            />
            <SummaryListRow
              label="Head office phone number"
              value={organisation.headOfficeTelephone}
            />
          </SummaryList>

          <Button variant="secondary" to={`/portal/organisations/${organisation.id}/edit`}>
            Edit details
          </Button>

          <TableAndFiltersGrid
            title="Search and filter"
            filters={<OrganisationFilters query={query} />}
            table={
              <Suspense fallback={<p>Loading users...</p>} key={buildUserListHref(query)}>
                <OrganisationUsersTable
                  apiClient={apiClient}
                  organisationId={organisation.id}
                  query={query}
                />
              </Suspense>
            }
          />
        </>
      )}
    </OrganisationPageWrapper>
  )
}
