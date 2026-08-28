import type { UserRole } from '@/client/generated/types.gen'

// Champions can only toggle a user between the standard and champion roles. Super users are
// managed outside of the organisation portal, so their role cannot be changed from here.
export const switchableRoles = ['Standard', 'Champion'] as const satisfies readonly UserRole[]

export type SwitchableRole = (typeof switchableRoles)[number]

export function isSwitchableRole(role: UserRole): role is SwitchableRole {
  return (switchableRoles as readonly UserRole[]).includes(role)
}

export function getSwitchedRole(role: SwitchableRole): SwitchableRole {
  return role === 'Standard' ? 'Champion' : 'Standard'
}

// Reads as part of a sentence, so unlike the labels in userLabels these are lower case.
export const roleDescriptions: Record<UserRole, string> = {
  Champion: 'a champion user',
  Standard: 'a standard user',
  Super: 'a super user',
}

// The button offers the role the user would be moved to, so it is labelled by the role they
// do not currently have.
export const switchRoleButtonLabels: Record<SwitchableRole, string> = {
  Champion: 'Make standard user',
  Standard: 'Make champion user',
}
