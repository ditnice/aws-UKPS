import Link from 'next/link'

import { PaginatedResponseDtoOfRecordListItemDto } from '@/client/generated'
import { Tag } from '@/components/Tag/Tag'

import { ApplicationTableWithPagination } from '../_components/ApplicationTable'

import {
  organisationRecordsTableHeaders,
  recordStatusLabels,
  recordStatusTagColours,
} from './labels'
import { convertQueryToSearchParams, RecordsQuery } from './recordsQuery'

type RecordsTableProps = {
  organisationId: number
  data: PaginatedResponseDtoOfRecordListItemDto
  query: RecordsQuery
}
const RecordsTable = async ({ organisationId, data: records, query }: RecordsTableProps) => {
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
                <Tag colour={recordStatusTagColours[data.displayStatus]}>
                  {recordStatusLabels[data.displayStatus]}
                </Tag>
              )
            case 'company-code':
              return <>{data.companyCode}</>
            case 'next-update':
              return <>TODO</>
            case 'records-title':
              return <>{data.title}</>
            case 'actions':
              return (
                <Link
                  href={`/portal/organisations/${organisationId}/records/${data.id}?recordType=${data.recordType}`}
                >
                  View<span className="visually-hidden"> record {data.id}</span>
                </Link>
              )
          }
        }}
      />
    </>
  )
}

export default RecordsTable
