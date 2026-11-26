import { TableAndFiltersGrid } from '@/app/portal/components/_components/TableAndFiltersGrid'
import { getCurrentOrganisationChampions, getUsersMe, UserRole } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import OrganisationPageWrapper from '../_components/OrganisationPageWrapper'

import { CreateRecordActionBanner } from './_components/CreateRecordActionBanner'
import { MfaSetupCompleteAlert } from './_components/MfaSetupCompleteAlert'
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
  const apiClient = await createServerApiClient()
  const { data: currentUser } = await getUsersMe({ client: apiClient })
  const isStandardUser = currentUser?.userRole === UserRole.STANDARD
  const canCreateRecords = currentUser && currentUser.userRole !== UserRole.SUPER

  let championEmails: string[] = []
  if (isStandardUser) {
    const { data } = await getCurrentOrganisationChampions({
      client: apiClient,
    })
    championEmails = data ?? []
  }

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <>
          <MfaSetupCompleteAlert />
          {recordAction && <RecordActionAlert action={recordAction} />}
          <PageHeader heading={`${organisation.organisationName}`} />
          {isStandardUser && championEmails.length > 0 && (
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
          {canCreateRecords && (
            <CreateRecordActionBanner
              organisationId={organisationId}
              allowedPharmaceuticalEntity={currentUser.allowedPharmaceuticalEntity}
            />
          )}
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
export default OrganisationRecordsPage
