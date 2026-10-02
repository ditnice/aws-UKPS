import { requireEnvironmentVariable } from './test-environment'

export const e2eRoles = ['standard', 'champion', 'super'] as const

export type E2eRole = (typeof e2eRoles)[number]

export type RoleCredentials = {
  email: string
  password: string
  totpSecret: string
}

// Matches any role tag, used to catch authenticated specs that were not given a role.
export const anyRoleGrep = new RegExp(`@(${e2eRoles.join('|')})\\b`)

export function roleTag(role: E2eRole): string {
  return `@${role}`
}

export function roleGrep(role: E2eRole): RegExp {
  return new RegExp(`@${role}\\b`)
}

export function authStatePathFor(role: E2eRole): string {
  return `tests/e2e/.auth/${role}.json`
}

function roleEnvironmentVariables(role: E2eRole) {
  const prefix = `E2E_${role.toUpperCase()}`

  return {
    email: `${prefix}_EMAIL`,
    password: `${prefix}_PASSWORD`,
    totpSecret: `${prefix}_TOTP_SECRET`,
  }
}

export function hasRoleCredentials(role: E2eRole): boolean {
  return Object.values(roleEnvironmentVariables(role)).every((name) =>
    Boolean(process.env[name]?.trim()),
  )
}

export function getRoleCredentials(role: E2eRole): RoleCredentials {
  const names = roleEnvironmentVariables(role)

  return {
    email: requireEnvironmentVariable(names.email),
    password: requireEnvironmentVariable(names.password),
    totpSecret: requireEnvironmentVariable(names.totpSecret),
  }
}

export function configuredRoles(): E2eRole[] {
  return e2eRoles.filter(hasRoleCredentials)
}
