'use client'

import { PaginatedResponseDtoOfUserListItemDto } from '@/client/generated'

import {
  buildQueryFromFilters,
  buildUserListSearchParams,
  getActiveFilters,
  UserListQuery,
} from '../_lib/userListQuery'

import { PaginatedResultsAndFilterSummary } from './PaginatedResultsAndFilterSummary'

type UserFilterSummaryProps = {
  query: UserListQuery
  users: PaginatedResponseDtoOfUserListItemDto | undefined
}
export const UserFilterSummary = ({ query, users }: UserFilterSummaryProps) => {
  return (
    <PaginatedResultsAndFilterSummary
      query={query}
      result={users}
      getActiveFilters={getActiveFilters}
      convertFiltersToQuery={buildQueryFromFilters}
      convertQueryToSearchParams={buildUserListSearchParams}
    />
  )
}
