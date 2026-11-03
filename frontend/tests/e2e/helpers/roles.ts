/* Each authenticated suite declares the user roles it runs as with these tags, and the config
 * runs it once in each matching role's project (authenticated-super, -champion, -standard). */
export const userRoles = ['Super', 'Champion', 'Standard'] as const
export type UserRole = (typeof userRoles)[number]

/** With dev authentication, the backend acts as this cookie's role for each request. */
export const devRoleCookieName = 'ukps_dev_role'

/* The users the backend seeds and signs in as for each role with dev authentication. They must
 * match DevAuthenticationClaims.RoleUsers in the backend. */
export const devRoleEmails: Record<UserRole, string> = {
  Super: 'example.user@email.com',
  Champion: 'dev.champion@email.com',
  Standard: 'dev.standard@email.com',
}

export function roleTag(role: UserRole): string {
  return `@role-${role.toLowerCase()}`
}

/** Matches the role's tag, but not a longer tag that starts with it. */
export function roleTagPattern(role: UserRole): RegExp {
  return new RegExp(`${roleTag(role)}(?![\\w-])`)
}

/** Matches a test tagged with any role. */
export const anyRoleTagPattern = new RegExp(
  `@role-(${userRoles.map((role) => role.toLowerCase()).join('|')})(?![\\w-])`,
)

/** Tags for a describe or test that runs once as each role, with any other tags, e.g.
 * `test.describe('…', { tag: forRoles(['Super', 'Champion'], mutatesDataTag) }, …)`. */
export function forRoles(roles: readonly UserRole[], ...otherTags: string[]): string[] {
  if (roles.length === 0) {
    throw new Error('forRoles needs at least one role')
  }
  return [...roles.map(roleTag), ...otherTags]
}

/** For suites that apply to every role: `forRoles(allRoles)`. */
export const allRoles = userRoles
