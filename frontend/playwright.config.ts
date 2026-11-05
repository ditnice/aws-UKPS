import { defineConfig, devices } from '@playwright/test'
import 'dotenv/config'

import {
  anyRoleTagPattern,
  devRoleCookieName,
  roleTagPattern,
  userRoles,
  type UserRole,
} from './tests/e2e/helpers/roles'
import {
  authStatePath,
  ephemeralStackOnlyTags,
  hasRoleCredentials,
  isAuthenticatedTargetAllowed,
  isDevAuthMode,
  isEphemeralStack,
  isFlagSet,
  isLocalBaseURL,
  organisationIdFor,
} from './tests/e2e/helpers/test-environment'

import type { TestOptions } from './tests/e2e/fixtures/test'

const localBaseURL = 'https://localhost:3000'
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? localBaseURL
const isLocal = isLocalBaseURL(baseURL)
/* The Docker Compose stack runs with frontend AUTHENTICATION_MODE=DEV and backend
 * DevAuthentication, so authenticated specs run without signing in. That bypasses the
 * credential and target checks below, so it's only allowed against a local target. */
const isDevAuth = isDevAuthMode()
if (isDevAuth && !isLocal) {
  throw new Error(
    `E2E_AUTH_MODE=dev only works against a local target, but PLAYWRIGHT_BASE_URL is ${baseURL}.`,
  )
}
/* Only the Docker Compose stack sets E2E_EPHEMERAL_STACK, as its data resets on every start.
 * Elsewhere every project skips @mutates-data and @visual tests. The filter is on each project
 * because a command-line --grep or --grep-invert can't remove a project's filter. */
const isEphemeral = isEphemeralStack()
if (isEphemeral && !(isLocal && isDevAuth)) {
  throw new Error(
    `E2E_EPHEMERAL_STACK only works with E2E_AUTH_MODE=dev against a local target, but PLAYWRIGHT_BASE_URL is ${baseURL}.`,
  )
}
const ephemeralStackFilter = isEphemeral ? {} : { grepInvert: ephemeralStackOnlyTags }
const localWebServerCommand = process.env.PLAYWRIGHT_WEB_SERVER_COMMAND ?? 'pnpm dev'
const localChromiumOptions = isLocal
  ? { launchOptions: { args: ['--ignore-certificate-errors'] } }
  : {}
const publicTestIgnore = [
  /auth\.setup\.ts/,
  /\.authenticated\.e2e\.spec\.ts/,
  /accessibility\/.*\.e2e\.spec\.ts/,
]
/* Each authenticated spec runs once in the project of every role it's tagged with (see
 * tests/e2e/helpers/roles.ts). A role's projects only exist when it can sign in: always with dev
 * authentication, otherwise when the role's credentials are set and the target is trusted. */
const authenticatedTestMatch = /.*\.authenticated\.e2e\.spec\.ts/
const runnableRoles = userRoles.filter(
  (role) =>
    isDevAuth ||
    (organisationIdFor(role) !== undefined &&
      hasRoleCredentials(role) &&
      isAuthenticatedTargetAllowed(baseURL)),
)
const skippedRoles = userRoles.filter((role) => !runnableRoles.includes(role))
if (skippedRoles.length > 0 && runnableRoles.length > 0) {
  console.info(
    `Skipping authenticated tests for ${skippedRoles.join(', ')}: set E2E_<ROLE>_EMAIL, _PASSWORD, _TOTP_SECRET and an organisation ID to run them.`,
  )
}

/* With dev authentication nobody signs in: this cookie tells the backend which seeded user
 * each request acts as. */
function devRoleStorageState(role: UserRole) {
  return {
    cookies: [
      {
        name: devRoleCookieName,
        value: role,
        domain: new URL(baseURL).hostname,
        path: '/',
        expires: -1,
        httpOnly: false,
        secure: false,
        sameSite: 'Lax' as const,
      },
    ],
    origins: [],
  }
}

function authenticatedProjects(role: UserRole) {
  const name = `authenticated-${role.toLowerCase()}`
  const use = { ...devices['Desktop Chrome'], ...localChromiumOptions, userRole: role }

  if (isDevAuth) {
    return [
      {
        name,
        ...ephemeralStackFilter,
        testMatch: authenticatedTestMatch,
        grep: roleTagPattern(role),
        use: { ...use, storageState: devRoleStorageState(role) },
      },
    ]
  }

  const setupName = `auth-setup-${role.toLowerCase()}`
  return [
    {
      name: setupName,
      testMatch: /auth\.setup\.ts/,
      grep: new RegExp(`authenticate the ${role.toLowerCase()} user`),
      use: { ...devices['Desktop Chrome'], ...localChromiumOptions },
    },
    {
      name,
      ...ephemeralStackFilter,
      testMatch: authenticatedTestMatch,
      grep: roleTagPattern(role),
      // Restricted-role access tests verify subjects with a separate Super session. It must be
      // freshly authenticated even when only the Champion or Standard project was selected.
      dependencies: [
        setupName,
        ...(role !== 'Super' && runnableRoles.includes('Super') ? ['auth-setup-super'] : []),
      ],
      use: { ...use, storageState: authStatePath(role) },
    },
  ]
}

const skipWebServer = isFlagSet('PLAYWRIGHT_SKIP_WEB_SERVER')

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig<TestOptions>({
  testDir: './tests/e2e',
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  /* PLAYWRIGHT_OUTPUT_DIR and PLAYWRIGHT_HTML_OUTPUT_DIR let the Docker Compose runner keep all
   * output inside one mounted folder; Playwright deletes and recreates these folders on each run. */
  outputDir: process.env.PLAYWRIGHT_OUTPUT_DIR ?? 'test-results',
  reporter: [
    ['list'],
    ['html', { open: 'never' }],
    ...(process.env.CI
      ? [
          [
            'junit',
            { outputFile: process.env.PLAYWRIGHT_JUNIT_OUTPUT_FILE ?? 'test-results/junit.xml' },
          ] as const,
        ]
      : []),
  ],
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL,

    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',

    /* The dev server uses a locally-trusted mkcert certificate, but this keeps
     * test runs robust on machines/CI where the mkcert CA isn't installed. */
    ignoreHTTPSErrors: isLocal,
  },
  projects: [
    {
      name: 'chromium',
      ...ephemeralStackFilter,
      testIgnore: publicTestIgnore,
      use: {
        ...devices['Desktop Chrome'],
        ...localChromiumOptions,
      },
    },
    {
      name: 'firefox',
      ...ephemeralStackFilter,
      testIgnore: publicTestIgnore,
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      ...ephemeralStackFilter,
      testIgnore: publicTestIgnore,
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'accessibility',
      ...ephemeralStackFilter,
      testIgnore: /\.authenticated\.e2e\.spec\.ts/,
      testMatch: /accessibility\/.*\.e2e\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        ...localChromiumOptions,
      },
    },
    ...runnableRoles.flatMap(authenticatedProjects),
    /* Every authenticated test without a role tag fails here, so none silently runs nowhere. It
     * fails before signing in, so it needs no credentials. */
    {
      name: 'authenticated-untagged',
      testMatch: authenticatedTestMatch,
      grepInvert: anyRoleTagPattern,
      use: { ...devices['Desktop Chrome'], requireRoleTag: true },
    },
  ],
  webServer:
    isLocal && !skipWebServer
      ? {
          command: localWebServerCommand,
          reuseExistingServer: true,
          url: baseURL,
          ignoreHTTPSErrors: true,
        }
      : undefined,
})
