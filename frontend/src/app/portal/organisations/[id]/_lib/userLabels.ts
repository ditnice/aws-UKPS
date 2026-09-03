import type { UserOrgStatus, UserRole } from '@/client/generated/types.gen'
import type { TagColour } from '@/components/Tag/Tag'

export const organisationUserTableHeaders = {
  email: 'Email Address',
  role: 'Role',
  status: 'Status',
  lastActive: 'Last Active',
  actions: 'Actions',
} as const

export const roleLabels: Record<UserRole, string> = {
  Champion: 'Champion user',
  Standard: 'Standard user',
  Super: 'Super user',
}

export const statusLabels: Record<UserOrgStatus, string> = {
  Active: 'Active',
  AwaitingSetup: 'Pending',
  Deactivated: 'Deactivated',
  Inactive: 'Inactive',
  Rejected: 'Rejected',
  RequestedAccess: 'Requested',
}

export const statusTagColours: Record<UserOrgStatus, TagColour> = {
  Active: 'green',
  AwaitingSetup: 'blue',
  Deactivated: 'purple',
  Inactive: 'red',
  Rejected: 'grey',
  RequestedAccess: 'yellow',
}

// Rejected is not filterable as the backend never returns it
export const filterableStatuses = (Object.keys(statusLabels) as UserOrgStatus[]).filter(
  (status) => status !== 'Rejected',
)

// Super user is not filterable via this UI
export const filterableRoles = (Object.keys(roleLabels) as UserRole[]).filter(
  (role) => role !== 'Super',
)

export const lastActivePresets = ['week', 'month', '6months', 'year'] as const

export type LastActivePreset = (typeof lastActivePresets)[number]

export const lastActiveLabels: Record<LastActivePreset, string> = {
  week: 'In the last week',
  month: 'In the last month',
  '6months': 'In the last 6 months',
  year: 'In the last year',
}

export const lastActivePresetDays: Record<LastActivePreset, number> = {
  week: 7,
  month: 30,
  '6months': 182,
  year: 365,
}
