import { TableAndFiltersGrid } from '@/app/portal/components/_components/TableAndFiltersGrid'
import { getCurrentOrganisationChampions, getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import OrganisationPageWrapper from '../_components/OrganisationPageWrapper'

import { CreateRecordActionBanner } from './_components/CreateRecordActionBanner'
import { RecordActionAlert } from './_components/RecordActionAlert'
import { parseRecordAction, type RecordActionSearchParams } from './_lib/recordActionAlert'
import { RecordsFetch } from './RecordsFetch'
import { OrganisationRecordsSearchParams, parseQueryFromSearchParams } from './recordsQuery'
import { RecordsQueryResultsSummary } from './RecordsQueryResultsSummary'
import RecordsTable from './RecordsTable'
import RecordsTablesFilters from './RecordsTablesFilters'

type OrganisationRecordsPage = {
  params: Promise<{ id: string }>
  searchParams: Promise<OrganisationRecordsSearchParams & RecordActionSearchParams>
}

const OrganisationRecordsPage = async ({ params, searchParams }: OrganisationRecordsPage) => {
  const { id: organisationId } = await params
  const resolvedSearchParams = await searchParams
  const query = parseQueryFromSearchParams(resolvedSearchParams)
  const recordAction = parseRecordAction(resolvedSearchParams)
  const { data: currentUser, error: currentUserError } = await getUsersMe({
    client: await createServerApiClient(),
  })
  if (currentUserError || !currentUser) {
    return (
      <section>
        <PageHeader heading="Unable to load current user" />
        <p role="alert">There was a problem retrieving the current user. Please try again.</p>
      </section>
    )
  }
  const { data: championEmails } = await getCurrentOrganisationChampions({
    client: await createServerApiClient(),
  })

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <>
          {recordAction && <RecordActionAlert action={recordAction} />}
          <PageHeader heading={`${organisation.organisationName}`} />
          {currentUser?.userRole === 'Standard' && championEmails && championEmails.length > 0 && (
            <>
              <div className="champion-user">
                {championEmails.length === 1 ? (
                  <p>
                    Your champion user is:{' '}
                    <a href={`mailto:${championEmails[0]}`}>{championEmails[0]}</a>
                  </p>
                ) : (
                  <>
                    <p>Your champion users are:</p>
                    <ul className="multiple-champion-users">
                      {championEmails.map((email) => (
                        <li key={email}>
                          <a href={`mailto:${email}`}>{email}</a>
                        </li>
                      ))}
                    </ul>
                  </>
                )}
              </div>
            </>
          )}
          <CreateRecordActionBanner
            organisationId={organisationId}
            allowedPharmaceuticalEntity={currentUser?.allowedPharmaceuticalEntity}
          />
          <TableAndFiltersGrid
            title="Search and filter records"
            filters={<RecordsTablesFilters query={query} />}
            table={
              <RecordsFetch organisationId={organisation.id} query={query}>
                {(data) => (
                  <>
                    <RecordsQueryResultsSummary query={query} data={data} />
                    <RecordsTable organisationId={organisation.id} data={data} query={query} />
                  </>
                )}
              </RecordsFetch>
            }
          />
        </>
      )}
    </OrganisationPageWrapper>
  )
}
// TODO: Add the record template to the link

export default OrganisationRecordsPage
