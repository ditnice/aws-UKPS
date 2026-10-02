import { ActionBanner } from '@nice-digital/nds-action-banner'

import { TableAndFiltersGrid } from '@/app/portal/components/_components/TableAndFiltersGrid'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import OrganisationPageWrapper from '../_components/OrganisationPageWrapper'

import { RecordsFetch } from './RecordsFetch'
import { OrganisationRecordsSearchParams, parseQueryFromSearchParams } from './recordsQuery'
import { RecordsQueryResultsSummary } from './RecordsQueryResultsSummary'
import RecordsTable from './RecordsTable'
import RecordsTablesFilters from './RecordsTablesFilters'

type OrganisationRecordsPage = {
  params: Promise<{ id: string }>
  searchParams: Promise<OrganisationRecordsSearchParams>
}
const OrganisationRecordsPage = async ({ params, searchParams }: OrganisationRecordsPage) => {
  const { id: organisationId } = await params
  const query = parseQueryFromSearchParams(await searchParams)

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <>
          <PageHeader heading={`${organisation.organisationName}`} />
          <CreateMedicineRecordActionBanner />
          <TableAndFiltersGrid
            title="Search and filter records"
            filters={<RecordsTablesFilters query={query} />}
            table={
              <RecordsFetch organisationId={organisation.id} query={query}>
                {(data) => (
                  <>
                    <RecordsQueryResultsSummary query={query} data={data} />
                    <RecordsTable data={data} query={query} />
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

const CreateMedicineRecordActionBanner = () => {
  return (
    <ActionBanner
      variant="subtle"
      title="Create Record"
      cta={
        <>
          <Button>Create medicine record (TODO)</Button>
        </>
      }
    >
      To create a new record select the record type you want to create.
    </ActionBanner>
  )
}

export default OrganisationRecordsPage
