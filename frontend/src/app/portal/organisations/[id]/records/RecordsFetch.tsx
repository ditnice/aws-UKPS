import { PaginatedResponseDtoOfRecordListItemDto, getOrganisationRecords } from '@/client/generated'
import { fakePaginatedResponseDtoOfRecordListItemDto } from '@/client/generated/@faker-js/faker.gen'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { RecordsQuery } from './recordsQuery'

export type RecordsFetchProps = {
  organisationId: number
  query: RecordsQuery
  children: (data: PaginatedResponseDtoOfRecordListItemDto) => React.ReactElement
}
export const RecordsFetch = async ({ organisationId, query, children }: RecordsFetchProps) => {
  const { data: records, error } = await getOrganisationRecords({
    path: { organisationId }, query: {
      Search: query.search, RecordType: query.recordType, RecordStatus: query.recordStatus, Page: query.page
      , PageSize: query.pageSize, SortBy: query.sortBy, SortDirection: query.sortDirection
    }
  })

  if (!records || error) {
    return (
      <ErrorState>There was a problem retrieving the records. Please try again later.</ErrorState>
    )
  }

  return children(records)
}
