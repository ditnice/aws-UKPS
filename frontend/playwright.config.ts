import { defineConfig, devices } from '@playwright/test'
import 'dotenv/config'

import {
  isAuthenticatedTargetAllowed,
  isDevAuthMode,
  isLocalBaseURL,
} from './tests/e2e/helpers/test-environment'

const localBaseURL = 'https://localhost:3000'
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? localBaseURL
const isLocal = isLocalBaseURL(baseURL)
/* The Docker Compose stack runs with frontend AUTHENTICATION_MODE=DEV and backend
 * DevAuthentication, so authenticated specs run without signing in. */
const isDevAuth = isDevAuthMode()
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
const authenticatedProjects = isDevAuth
  ? [
      {
        name: 'authenticated-chromium',
        testMatch: /.*\.authenticated\.e2e\.spec\.ts/,
        use: {
          ...devices['Desktop Chrome'],
          ...localChromiumOptions,
        },
      },
    ]
  : [
      {
        name: 'auth-setup',
        testMatch: /auth\.setup\.ts/,
        use: {
          ...devices['Desktop Chrome'],
          ...localChromiumOptions,
        },
      },
      {
        name: 'authenticated-chromium',
        dependencies: ['auth-setup'],
        testMatch: /.*\.authenticated\.e2e\.spec\.ts/,
        use: {
          ...devices['Desktop Chrome'],
          ...localChromiumOptions,
          storageState: 'tests/e2e/.auth/authenticated-dev.json',
        },
      },
    ]
const skipWebServer = Boolean(process.env.PLAYWRIGHT_SKIP_WEB_SERVER)

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
