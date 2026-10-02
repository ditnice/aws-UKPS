import { defineConfig, devices } from '@playwright/test'
import 'dotenv/config'

import { anyRoleGrep, authStatePathFor, configuredRoles, roleGrep } from './tests/e2e/helpers/roles'
import { isAuthenticatedTargetAllowed, isLocalBaseURL } from './tests/e2e/helpers/test-environment'

const localBaseURL = 'https://localhost:3000'
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? localBaseURL
const isLocal = isLocalBaseURL(baseURL)
const localWebServerCommand = process.env.PLAYWRIGHT_WEB_SERVER_COMMAND ?? 'pnpm dev'
const localChromiumOptions = isLocal
  ? { launchOptions: { args: ['--ignore-certificate-errors'] } }
  : {}
const publicTestIgnore = [
  /auth\.setup\.ts/,
  /\.authenticated\.e2e\.spec\.ts/,
  /accessibility\/.*\.e2e\.spec\.ts/,
]
const authenticatedTestMatch = /.*\.authenticated\.e2e\.spec\.ts/
const canRunAuthenticatedTests =
  Boolean(process.env.E2E_ORGANISATION_ID?.trim()) && isAuthenticatedTargetAllowed(baseURL)
// Each role with credentials gets its own sign-in setup and test project, so a spec
// tagged with several roles runs once per role.
const authenticatedRoles = canRunAuthenticatedTests ? configuredRoles() : []

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
  reporter: [['list'], ['html', { open: 'never' }]],
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
      testIgnore: publicTestIgnore,
      use: {
        ...devices['Desktop Chrome'],
        ...localChromiumOptions,
      },
    },
    {
      name: 'firefox',
      testIgnore: publicTestIgnore,
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      testIgnore: publicTestIgnore,
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'accessibility',
      testIgnore: /\.authenticated\.e2e\.spec\.ts/,
      testMatch: /accessibility\/.*\.e2e\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        ...localChromiumOptions,
      },
    },
    ...authenticatedRoles.flatMap((role) => [
      {
        name: `auth-setup-${role}`,
        testMatch: /auth\.setup\.ts/,
        grep: roleGrep(role),
        use: {
          ...devices['Desktop Chrome'],
          ...localChromiumOptions,
        },
      },
      {
        name: `authenticated-${role}`,
        dependencies: [`auth-setup-${role}`],
        testMatch: authenticatedTestMatch,
        grep: roleGrep(role),
        metadata: { role },
        use: {
          ...devices['Desktop Chrome'],
          ...localChromiumOptions,
          storageState: authStatePathFor(role),
        },
      },
    ]),
    {
      // Authenticated specs without a role tag would otherwise never run. This project
      // collects them so the fixture in tests/e2e/fixtures/test.ts can fail them.
      name: 'authenticated-untagged',
      testMatch: authenticatedTestMatch,
      grepInvert: anyRoleGrep,
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: isLocal
    ? {
        command: localWebServerCommand,
        reuseExistingServer: true,
        url: baseURL,
        ignoreHTTPSErrors: true,
      }
    : undefined,
})
