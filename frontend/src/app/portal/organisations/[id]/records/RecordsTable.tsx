import { PaginatedResponseDtoOfRecordListItemDto } from '@/client/generated'

import { ApplicationTableWithPagination } from '../_components/ApplicationTable'

import { organisationRecordsTableHeaders, recordStatusLabels } from './labels'
import { convertQueryToSearchParams, RecordsQuery } from './recordsQuery'

type RecordsTableProps = {
  data: PaginatedResponseDtoOfRecordListItemDto
  query: RecordsQuery
}
const RecordsTable = async ({ data: records, query }: RecordsTableProps) => {
  return (
    <>
      <ApplicationTableWithPagination
        captionName="Organisation Records"
        result={records}
        getItemKey={(x) => x.id}
        headers={organisationRecordsTableHeaders}
        query={query}
        queryToSearchParams={convertQueryToSearchParams}
        fallbackText="No records found for this organisation"
        getData={(key, data) => {
          switch (key) {
            case 'id':
              return <>{data.niceTaDevelopmentId}</>
            case 'record-status':
              return <>{recordStatusLabels[data.recordStatus]}</>
            case 'development-name':
            case 'next-update':
            case 'records-title':
            case 'actions':
              return <>TODO</>
          }
        }}
      />
    </>
  )
}

export default RecordsTable
