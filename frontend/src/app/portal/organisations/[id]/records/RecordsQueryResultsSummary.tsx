'use client'

import { PaginatedResponseDtoOfRecordListItemDto } from '@/client/generated'

import { PaginatedResultsAndFilterSummary } from '../_components/PaginatedResultsAndFilterSummary'

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
    <PaginatedResultsAndFilterSummary
      query={query}
      result={data}
      getActiveFilters={getActiveFilters}
      convertFiltersToQuery={buildQueryFromFilters}
      convertQueryToSearchParams={convertQueryToSearchParams}
    />
  )
}
