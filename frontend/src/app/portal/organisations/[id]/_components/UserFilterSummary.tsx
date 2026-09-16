'use client'

import { PaginatedResponseDtoOfUserListItemDto } from '@/client/generated'

import {
  buildQueryFromFilters,
  buildUserListSearchParams,
  getActiveFilters,
  UserListQuery,
} from '../_lib/userListQuery'

import { PaginatedResultFilterSummary } from './PaginatedResultFilterSummary'

type UserFilterSummaryProps = {
  query: UserListQuery
  users: PaginatedResponseDtoOfUserListItemDto | undefined
}
export const UserFilterSummary = ({ query, users }: UserFilterSummaryProps) => {
  return (
    <PaginatedResultFilterSummary
      query={query}
      result={users}
      getActiveFilters={getActiveFilters}
      convertFiltersToQuery={buildQueryFromFilters}
      convertQueryToSearchParams={buildUserListSearchParams}
    />
  )
}
