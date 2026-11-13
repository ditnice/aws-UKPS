import Link from 'next/link'

import { TableAndFiltersGrid } from '@/app/portal/components/_components/TableAndFiltersGrid'
import { getCurrentOrganisationChampions, getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { ActionBanner } from '@/components/ActionBanner/ActionBanner'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import OrganisationPageWrapper from '../_components/OrganisationPageWrapper'

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
  const { data: championEmails } = await getCurrentOrganisationChampions({
    client: apiClient,
  })

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <>
          {recordAction && <RecordActionAlert action={recordAction} />}
          <PageHeader heading={`${organisation.organisationName}`} />

          {currentUser?.userRole === 'Standard' && championEmails && championEmails.length > 0 && (
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
          )}
          <CreateMedicineRecordActionBanner organisationId={organisationId} />
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

const CreateMedicineRecordActionBanner = ({ organisationId }: { organisationId: string }) => {
  return (
    <ActionBanner
      variant="subtle"
      title="Create Record"
      cta={
        <>
          <Button
            elementType={Link}
            href={`/portal/organisations/${organisationId}/records/create`}
          >
            Create medicine record
          </Button>
        </>
      }
    >
      To create a new record select the record type you want to create.{' '}
      <a>View a record template (opens in new tab)</a> to check what information you will need to
      provide.
    </ActionBanner>
  )
}

export default OrganisationRecordsPage
