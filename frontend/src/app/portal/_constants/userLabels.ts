import type { UserOrgStatus, UserRole } from '@/client/generated/types.gen'
import type { TagColour } from '@/components/Tag/Tag'

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
