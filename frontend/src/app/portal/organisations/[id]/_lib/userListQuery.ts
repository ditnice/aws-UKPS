import type { UserOrgStatus, UserRole } from '@/client/generated/types.gen'
import { parsePage, parsePageSize } from '@/lib/search-and-filter/pagination'
import { parseMulti } from '@/lib/search-and-filter/query'

import {
  filterableRoles,
  filterableStatuses,
  lastActivePresets,
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
}

// The validated user list state, shared by the page and the table
export interface UserListQuery {
  page: number
  pageSize: number
  status: UserOrgStatus[]
  role: UserRole[]
  email?: string
  lastActive?: LastActivePreset
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

export function parseUserListQuery(searchParams: UserListSearchParams): UserListQuery {
  return {
    page: parsePage(searchParams.page),
    pageSize: parsePageSize(searchParams.pageSize),
    status: parseMulti(searchParams.status, filterableStatuses),
    role: parseMulti(searchParams.role, filterableRoles),
    email: parseEmail(searchParams.email),
    lastActive: parseLastActive(searchParams.lastActive),
  }
}

export function buildUserListSearchParams({
  page,
  pageSize,
  status,
  role,
  email,
  lastActive,
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

  return params
}

export function buildUserListHref(query: UserListQuery): string {
  return `?${buildUserListSearchParams(query).toString()}`
}
