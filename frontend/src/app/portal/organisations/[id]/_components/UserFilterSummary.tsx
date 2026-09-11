'use client'

import { useRouter } from 'next/navigation'

import { FilterSummary } from '@nice-digital/nds-filters'

import { PaginatedResponseDtoOfUserListItemDto } from '@/client/generated'

import {
  buildUserListHref,
  getActiveFilters,
  getUpdatedQueryWithoutFilter,
  UserListQuery,
} from '../_lib/userListQuery'
import styles from '../page.module.scss'

const getFirstResult = (totalCount: number, currentPage: number, pageSize: number): number => {
  return totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1
}

const getLastResult = (totalCount: number, currentPage: number, pageSize: number): number => {
  return Math.min(currentPage * pageSize, totalCount)
}

type UserFilterSummaryProps = {
  query: UserListQuery
  users: PaginatedResponseDtoOfUserListItemDto | undefined
}
export const UserFilterSummary = ({ query, users }: UserFilterSummaryProps) => {
  const router = useRouter()
  const { page, pageSize } = query
  const totalCount = users?.totalCount ?? 0
  const activeFilters = getActiveFilters(query)
  const activeFilterItems = activeFilters.map((af) => {
    const updatedQuery = getUpdatedQueryWithoutFilter(query, af)
    const destination = buildUserListHref({ ...updatedQuery, page: 1 })
    return { ...af, onClick: () => router.push(destination, { scroll: false }) }
  })
  return (
    <FilterSummary className={styles['users-filter-summary']} activeFilters={activeFilterItems}>
      {users
        ? `Showing results ${getFirstResult(totalCount, page, pageSize)} to ${getLastResult(totalCount, page, pageSize)} of ${totalCount}`
        : 'Showing results'}
    </FilterSummary>
  )
}
