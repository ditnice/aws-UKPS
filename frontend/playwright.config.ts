import { defineConfig, devices } from '@playwright/test'
import 'dotenv/config'

const localBaseURL = 'https://localhost:3000'
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? localBaseURL
const isLocal = new URL(baseURL).hostname === 'localhost'
const localChromiumOptions = isLocal
  ? { launchOptions: { args: ['--ignore-certificate-errors'] } }
  : {}

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './tests/e2e',
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: 'html',
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL,

    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',

    /* The dev server uses a locally-trusted mkcert certificate, but this keeps
     * test runs robust on machines/CI where the mkcert CA isn't installed. */
    ignoreHTTPSErrors: isLocal,
  },
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        ...localChromiumOptions,
      },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'Google Chrome',
      use: {
        ...devices['Desktop Chrome'],
        channel: 'chrome',
        ...localChromiumOptions,
      },
    },
    {
      name: 'Microsoft Edge',
      use: {
        ...devices['Desktop Edge'],
        channel: 'msedge',
        ...localChromiumOptions,
      },
    },
  ],
  webServer: isLocal
    ? {
        command: 'pnpm dev',
        reuseExistingServer: true,
        url: localBaseURL,
        ignoreHTTPSErrors: true,
      }
    : undefined,
})
