export const authStatePath = 'tests/e2e/.auth/authenticated-dev.json'

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
export const ephemeralStackOnlyTags = /@mutates-data|@visual/

export function isEphemeralStack(): boolean {
  return ['1', 'true'].includes(process.env.E2E_EPHEMERAL_STACK ?? '')
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
