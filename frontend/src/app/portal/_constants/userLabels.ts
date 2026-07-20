import type { UserOrgStatus, UserRole } from '@/client/generated/types.gen'

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
