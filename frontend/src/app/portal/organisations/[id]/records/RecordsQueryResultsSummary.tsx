'use client'

import { PaginatedResponseDtoOfRecordListItemDto } from '@/client/generated'

import { PaginatedResultFilterSummary } from '../_components/PaginatedResultFilterSummary'

import {
  buildQueryFromFilters,
  convertQueryToSearchParams,
  getActiveFilters,
  RecordsQuery,
} from './recordsQuery'

type RecordsQueryResultsSummaryProps = {
  query: RecordsQuery
  data: PaginatedResponseDtoOfRecordListItemDto
}
export const RecordsQueryResultsSummary = ({ query, data }: RecordsQueryResultsSummaryProps) => {
  return (
    <PaginatedResultFilterSummary
      query={query}
      result={data}
      getActiveFilters={getActiveFilters}
      convertFiltersToQuery={buildQueryFromFilters}
      convertQueryToSearchParams={convertQueryToSearchParams}
    />
  )
}
