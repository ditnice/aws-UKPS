'use client'

import { PaginatedResponseDtoOfUserListItemDto } from '@/client/generated'

import {
  buildUserQueryFromFilters,
  buildUserListSearchParams,
  getActiveUserFilters,
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
      getActiveFilters={getActiveUserFilters}
      convertFiltersToQuery={buildUserQueryFromFilters}
      convertQueryToSearchParams={buildUserListSearchParams}
    />
  )
}
