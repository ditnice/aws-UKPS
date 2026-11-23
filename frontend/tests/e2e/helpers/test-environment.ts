import { devRoleEmails, devRoleOrganisationIds, type UserRole } from './roles'

/** Where auth-setup saves each role's signed-in browser state. */
export function authStatePath(role: UserRole): string {
  return `tests/e2e/.auth/${role.toLowerCase()}.json`
}

export type RoleCredentials = {
  email: string
  password: string
  totpSecret: string
}

type CredentialVariables = Record<keyof RoleCredentials, string>

function roleCredentialVariables(prefix: string): CredentialVariables {
  return {
    email: `${prefix}_EMAIL`,
    password: `${prefix}_PASSWORD`,
    totpSecret: `${prefix}_TOTP_SECRET`,
  }
}

// The variables the one authenticated user had before each role had its own.
const legacySuperVariables: CredentialVariables = {
  email: 'E2E_USER_EMAIL',
  password: 'E2E_USER_PASSWORD',
  totpSecret: 'E2E_TOTP_SECRET',
}

function readVariable(name: string): string | undefined {
  return process.env[name]?.trim() || undefined
}

/* Each role signs in with E2E_<ROLE>_EMAIL, _PASSWORD and _TOTP_SECRET. Super falls back to
 * E2E_USER_EMAIL, E2E_USER_PASSWORD and E2E_TOTP_SECRET. A role is only configured when all
 * three of one set are set. */
export function roleCredentials(role: UserRole): RoleCredentials | undefined {
  const variableSets = [roleCredentialVariables(`E2E_${role.toUpperCase()}`)]
  if (role === 'Super') variableSets.push(legacySuperVariables)

  for (const variables of variableSets) {
    const email = readVariable(variables.email)
    const password = readVariable(variables.password)
    const totpSecret = readVariable(variables.totpSecret)
    if (email && password && totpSecret) {
      return { email, password, totpSecret }
    }
  }
  return undefined
}

export function hasRoleCredentials(role: UserRole): boolean {
  return roleCredentials(role) !== undefined
}

/** The organisation a role's tests act on: E2E_<ROLE>_ORGANISATION_ID if set; otherwise, with dev
 * authentication, the organisation the backend seeds that role's user in; otherwise
 * E2E_ORGANISATION_ID. */
export function organisationIdFor(role: UserRole): string | undefined {
  return (
    readVariable(`E2E_${role.toUpperCase()}_ORGANISATION_ID`) ??
    (isDevAuthMode() ? devRoleOrganisationIds[role] : undefined) ??
    readVariable('E2E_ORGANISATION_ID')
  )
}

/** The email of the user a role's tests are signed in as, so tests can avoid changing them. */
export function signedInEmail(role: UserRole): string | undefined {
  return isDevAuthMode() ? devRoleEmails[role] : roleCredentials(role)?.email
}

const defaultAuthenticatedOrigins = ['https://dev.ukps.nice.org.uk']

function getAuthenticatedOrigins(): Set<string> {
  const configuredOrigins = process.env.PLAYWRIGHT_AUTHENTICATED_ORIGINS?.split(',')
    .map((origin) => origin.trim())
    .filter(Boolean)
  const origins = configuredOrigins?.length ? configuredOrigins : defaultAuthenticatedOrigins

  return new Set(
    origins.map((origin) => {
      try {
        return new URL(origin).origin
      } catch {
        throw new Error(`PLAYWRIGHT_AUTHENTICATED_ORIGINS contains an invalid URL: ${origin}`)
      }
    }),
  )
}

export function isAuthenticatedTargetAllowed(baseURL: string | undefined): boolean {
  return Boolean(baseURL && getAuthenticatedOrigins().has(new URL(baseURL).origin))
}

export function isDevAuthMode(): boolean {
  return process.env.E2E_AUTH_MODE === 'dev'
}

/* Tests with these tags only run on the ephemeral Docker Compose stack (`just e2e`), which resets
 * its data on every start, so they never change a deployed environment's data or compare its
 * rendering against Linux Chromium baselines. */
export const mutatesDataTag = '@mutates-data'
export const visualTag = '@visual'
// Not followed by another tag character, so a tag such as @visual-regression wouldn't match.
export const ephemeralStackOnlyTags = new RegExp(`(${mutatesDataTag}|${visualTag})(?![\\w-])`)

/** For on/off variables such as PLAYWRIGHT_SKIP_WEB_SERVER: on only when set to `true`. */
export function isFlagSet(name: string): boolean {
  return process.env[name] === 'true'
}

export function isEphemeralStack(): boolean {
  return isFlagSet('E2E_EPHEMERAL_STACK')
}

export function isLocalBaseURL(baseURL: string | undefined): boolean {
  return Boolean(baseURL && new URL(baseURL).hostname === 'localhost')
}

export function requireAuthenticatedTarget(baseURL: string | undefined): void {
  if (!isAuthenticatedTargetAllowed(baseURL)) {
    const target = baseURL ? new URL(baseURL).origin : 'an unconfigured origin'
    throw new Error(
      `Refusing to submit authenticated Playwright credentials to ${target}. ` +
        'Add the trusted origin to PLAYWRIGHT_AUTHENTICATED_ORIGINS to allow it.',
    )
  }
}

export function requireEnvironmentVariable(name: string): string {
  const value = process.env[name]?.trim()

  if (!value) {
    throw new Error(`${name} must be set to run authenticated Playwright tests.`)
  }

  return value
}
