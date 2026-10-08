import { PaginatedResponseDtoOfRecordListItemDto } from '@/client/generated'
import type { RecordListItemDto } from '@/client/generated/types.gen'
import { Tag } from '@/components/Tag/Tag'

import { ApplicationTableWithPagination } from '../_components/ApplicationTable'

import {
  organisationRecordsTableHeaders,
  recordStatusLabels,
  recordStatusTagColours,
  workflowStatusLabels,
  workflowStatusTagColours,
} from './labels'
import { convertQueryToSearchParams, RecordsQuery } from './recordsQuery'

type RecordsTableProps = {
  data: PaginatedResponseDtoOfRecordListItemDto
  query: RecordsQuery
}
function renderNextUpdate(nextUpdateDue: RecordListItemDto['nextUpdateDue']) {
  if (!nextUpdateDue) {
    return 'Not applicable'
  }

  const dueDate = new Date(nextUpdateDue)
  if (dueDate < new Date()) {
    return <Tag colour="red">Overdue</Tag>
  }

  return new Intl.DateTimeFormat('en-GB').format(dueDate)
}

function renderStatus(record: RecordListItemDto) {
  if (record.recordStatus === 'OnHold' || record.recordStatus === 'Archived') {
    return (
      <Tag colour={recordStatusTagColours[record.recordStatus]}>
        {recordStatusLabels[record.recordStatus]}
      </Tag>
    )
  }

  return (
    <Tag colour={workflowStatusTagColours[record.workflowStatus]}>
      {workflowStatusLabels[record.workflowStatus]}
    </Tag>
  )
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
              return <>{data.id}</>
            case 'development-name':
              return <>{data.developmentName}</>
            case 'next-update':
              return <>{renderNextUpdate(data.nextUpdateDue)}</>
            case 'record-status':
              return <>{renderStatus(data)}</>
            case 'record-title':
              return <>{data.title}</>
            case 'actions':
              return <>TODO</>
          }
        }}
      />
    </>
  )
}

export default RecordsTable
