import type { UserOrgStatus, UserRole } from '@/client/generated/types.gen'
import type { TagColour } from '@/components/Tag/Tag'

export const roleLabels: Record<UserRole, string> = {
  Champion: 'Champion user',
  Standard: 'Standard user',
  Super: 'Super user',
}

export const statusLabels: Record<UserOrgStatus, string> = {
  Active: 'Active',
  AwaitingSetup: 'Awaiting setup',
  Inactive: 'Inactive',
  Rejected: 'Rejected',
  RequestedAccess: 'Requested access',
}

export const statusTagColours: Record<UserOrgStatus, TagColour> = {
  Active: 'green',
  AwaitingSetup: 'teal',
  Inactive: 'red',
  Rejected: 'grey',
  RequestedAccess: 'yellow',
}
