import type {
  GetUsersQuerySortValue,
  SortDirection,
  UserOrgStatus,
  UserRole,
} from '@/client/generated/types.gen'
import { parsePage, parsePageSize } from '@/lib/search-and-filter/pagination'
import { parseMulti, parseSortDirection } from '@/lib/search-and-filter/query'

import {
  filterableRoles,
  filterableStatuses,
  lastActiveLabels,
  lastActivePresets,
  roleLabels,
  statusLabels,
  type LastActivePreset,
} from './userLabels'

// The raw, unvalidated query string values as Next.js provides it
export interface UserListSearchParams {
  page?: string
  pageSize?: string
  status?: string | string[]
  role?: string | string[]
  email?: string
  lastActive?: string
  sortBy?: string
  sortDirection?: string
}

// The validated user list state, shared by the page and the table
export interface UserListQuery {
  page: number
  pageSize: number
  status: UserOrgStatus[]
  role: UserRole[]
  email?: string
  lastActive?: LastActivePreset
  sortBy?: GetUsersQuerySortValue
  sortDirection?: SortDirection
}

function parseEmail(email: string | undefined): string | undefined {
  return email?.trim() || undefined
}

function isLastActivePreset(value: string | undefined): value is LastActivePreset {
  return lastActivePresets.includes(value as LastActivePreset)
}

function parseLastActive(lastActive: string | undefined): LastActivePreset | undefined {
  return isLastActivePreset(lastActive) ? lastActive : undefined
}

function parseSortBy(sortBy: string | undefined): GetUsersQuerySortValue | undefined {
  const validSortValues: GetUsersQuerySortValue[] = ['Email', 'Role', 'Status', 'LastActive']
  return validSortValues.includes(sortBy as GetUsersQuerySortValue)
    ? (sortBy as GetUsersQuerySortValue)
    : undefined
}

export function parseUserListQuery(searchParams: UserListSearchParams): UserListQuery {
  return {
    page: parsePage(searchParams.page),
    pageSize: parsePageSize(searchParams.pageSize),
    status: parseMulti(searchParams.status, filterableStatuses),
    role: parseMulti(searchParams.role, filterableRoles),
    email: parseEmail(searchParams.email),
    lastActive: parseLastActive(searchParams.lastActive),
    sortBy: parseSortBy(searchParams.sortBy) ?? 'LastActive',
    sortDirection: parseSortDirection(searchParams.sortDirection) ?? 'Descending',
  }
}

type Filter = (
  | {
      key: 'email'
      value: string
    }
  | {
      key: 'status'
      value: UserOrgStatus
    }
  | {
      key: 'role'
      value: UserRole
    }
  | {
      key: 'last-active'
      value: LastActivePreset
    }
) & { label: string }
export const getActiveFilters = (query: UserListQuery): Filter[] => {
  return [
    ...query.status.map((s) => ({ key: 'status', value: s, label: statusLabels[s] }) as const),
    ...query.role.map((r) => ({ key: 'role', value: r, label: roleLabels[r] }) as const),
    ...(query.email ? [{ key: 'email', value: query.email, label: query.email } as const] : []),
    ...(query.lastActive
      ? [
          {
            key: 'last-active',
            value: query.lastActive,
            label: lastActiveLabels[query.lastActive],
          } as const,
        ]
      : []),
  ]
}

export const getUpdatedQueryWithoutFilter = (
  query: UserListQuery,
  filter: Filter,
): UserListQuery => {
  const activeFilters = getActiveFilters(query)
  const remainingFilters = activeFilters.filter(
    (x) => x.key !== filter.key || x.value !== filter.value,
  )
  return buildQueryFromFilters(remainingFilters, query)
}

const buildQueryFromFilters = (filters: Filter[], initialQuery: UserListQuery): UserListQuery => ({
  ...initialQuery,
  status: filters.filter((filter) => filter.key === 'status').map((filter) => filter.value),
  role: filters.filter((filter) => filter.key === 'role').map((filter) => filter.value),
  email: filters.find((filter) => filter.key === 'email')?.value,
  lastActive: filters.find((filter) => filter.key === 'last-active')?.value,
})

export const getNumberOfActiveFilters = (query: UserListQuery): number => {
  return getActiveFilters(query).length
}

export function buildUserListSearchParams({
  page,
  pageSize,
  status,
  role,
  email,
  lastActive,
  sortBy,
  sortDirection,
}: UserListQuery): URLSearchParams {
  const params = new URLSearchParams()
  params.set('page', String(page))
  params.set('pageSize', String(pageSize))
  status.forEach((value) => params.append('status', value))
  role.forEach((value) => params.append('role', value))
  if (email) {
    params.set('email', email)
  }
  if (lastActive) {
    params.set('lastActive', lastActive)
  }
  if (sortBy) {
    params.set('sortBy', sortBy)
  }
  if (sortDirection) {
    params.set('sortDirection', sortDirection)
  }

  return params
}

export function buildUserListHref(query: UserListQuery): string {
  return `?${buildUserListSearchParams(query).toString()}`
}
