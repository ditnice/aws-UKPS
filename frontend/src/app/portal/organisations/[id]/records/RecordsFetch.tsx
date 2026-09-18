import { PaginatedResponseDtoOfRecordListItemDto } from '@/client/generated'
import { fakePaginatedResponseDtoOfRecordListItemDto } from '@/client/generated/@faker-js/faker.gen'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { RecordsQuery } from './recordsQuery'

const mockGetRecords = (input: { query: RecordsQuery }) => {
  return {
    data: fakePaginatedResponseDtoOfRecordListItemDto(),
    error: undefined,
  }
}

export type RecordsFetchProps = {
  query: RecordsQuery
  children: (data: PaginatedResponseDtoOfRecordListItemDto) => React.ReactElement
}
export const RecordsFetch = async ({ query, children }: RecordsFetchProps) => {
  // TODO: Replace with getRecords when it is implemented.
  const { data: records, error } = await mockGetRecords({ query })

  if (!records || error) {
    return (
      <ErrorState>There was a problem retrieving the records. Please try again later.</ErrorState>
    )
  }

  return children(records)
}
