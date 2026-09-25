import { PaginatedResponseDtoOfRecordListItemDto, RecordStatus } from '@/client/generated'
import { Tag, TagColour } from '@/components/Tag/Tag'

import { ApplicationTableWithPagination } from '../_components/ApplicationTable'

import { organisationRecordsTableHeaders, recordStatusLabels } from './labels'
import { convertQueryToSearchParams, RecordsQuery } from './recordsQuery'

type RecordsTableProps = {
  data: PaginatedResponseDtoOfRecordListItemDto
  query: RecordsQuery
}
const RecordsTable = async ({ data: records, query }: RecordsTableProps) => {
  const recordStatusToTagLabelMap: Record<RecordStatus, TagColour> = {
    Unpublished: 'grey',
    Active: 'green',
    OnHold: 'orange',
    Archived: 'grey',
  }
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
              return <>{data.id}</>
            case 'record-status':
              return (
                <Tag colour={recordStatusToTagLabelMap[data.recordStatus]}>
                  {recordStatusLabels[data.recordStatus]}
                </Tag>
              )
            case 'development-name':
              return <>{data.developmentName}</>
            case 'next-update':
              return <>TODO</>
            case 'records-title':
              return <>TODO</>
            case 'actions':
              return <>TODO</>
          }
        }}
      />
    </>
  )
}

export default RecordsTable
