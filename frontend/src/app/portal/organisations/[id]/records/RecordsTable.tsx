import Link from 'next/link'

import { PaginatedResponseDtoOfRecordListItemDto, RecordListItemDto } from '@/client/generated'
import { recordPageHref } from '@/lib/dynamic-form/links'

import { ApplicationTableWithPagination } from '../_components/ApplicationTable'

import { organisationRecordsTableHeaders, recordStatusLabels } from './labels'
import { convertQueryToSearchParams, RecordsQuery } from './recordsQuery'

// Opens the record's form at its first page; each page loads the saved answers, so the user can
// step back through and change them (or view them, if the revision is no longer a draft).
const RecordAction = ({ record }: { record: RecordListItemDto }) =>
  record.firstPageId ? (
    <Link href={recordPageHref(record.id, record.latestRevisionId, record.firstPageId)}>
      View or edit <span className="visually-hidden">{record.title}</span>
    </Link>
  ) : null

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
              return <>{data.developmentName}</>
            case 'record-status':
              return <>{recordStatusLabels[data.recordStatus]}</>
            case 'actions':
              return <RecordAction record={data} />
            case 'development-name':
            case 'next-update':
            case 'records-title':
              return <>TODO</>
          }
        }}
      />
    </>
  )
}

export default RecordsTable
