import { defineConfig, devices } from '@playwright/test'
import 'dotenv/config'

import {
  ephemeralStackOnlyTags,
  isAuthenticatedTargetAllowed,
  isDevAuthMode,
  isEphemeralStack,
  isLocalBaseURL,
} from './tests/e2e/helpers/test-environment'

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
const hasAuthenticatedCredentials = [
  'E2E_USER_EMAIL',
  'E2E_USER_PASSWORD',
  'E2E_TOTP_SECRET',
  'E2E_ORGANISATION_ID',
].every((name) => Boolean(process.env[name]?.trim()))
const canRunAuthenticatedTests =
  isDevAuth || (hasAuthenticatedCredentials && isAuthenticatedTargetAllowed(baseURL))
const authenticatedChromiumProject = {
  name: 'authenticated-chromium',
  ...ephemeralStackFilter,
  testMatch: /.*\.authenticated\.e2e\.spec\.ts/,
  use: {
    ...devices['Desktop Chrome'],
    ...localChromiumOptions,
  },
}
const authenticatedProjects = isDevAuth
  ? [authenticatedChromiumProject]
  : [
      {
        name: 'auth-setup',
        ...ephemeralStackFilter,
        testMatch: /auth\.setup\.ts/,
        use: {
          ...devices['Desktop Chrome'],
          ...localChromiumOptions,
        },
      },
      {
        ...authenticatedChromiumProject,
        dependencies: ['auth-setup'],
        use: {
          ...authenticatedChromiumProject.use,
          storageState: 'tests/e2e/.auth/authenticated-dev.json',
        },
      },
    ]
const skipWebServer = ['1', 'true'].includes(process.env.PLAYWRIGHT_SKIP_WEB_SERVER ?? '')

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
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
    ...(canRunAuthenticatedTests ? authenticatedProjects : []),
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
