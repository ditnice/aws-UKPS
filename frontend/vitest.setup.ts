import { afterEach, beforeEach, vi } from 'vitest'

import { resetNextNavigation } from './src/test-utils/nextNavigation'

// Deterministic nonsecret configuration, independent of a developer's .env.
Object.assign(process.env, {
  BACKEND_API_BASE_URL: 'https://api.example.test',
  BACKEND_API_TIMEOUT_MS: '15000',
  PAYLOAD_SECRET: 'unit-test-only-secret',
  DATABASE_URL: 'postgres://test:test@127.0.0.1:5432/unit_test',
  COGNITO_ISSUER: 'https://issuer.example.test/pool',
  COGNITO_CLIENT_ID: 'unit-test-client',
  NEXT_PUBLIC_QA_SUPPORT_EMAIL: 'support@example.com',
})
delete process.env.AUTHENTICATION_MODE
delete process.env.FRONTEND_PUBLIC_ORIGIN

beforeEach(() => {
  // Clear implementations and queued one-shot responses as well as call histories.
  // vi.fn(initialImplementation) retains its original default after a reset.
  vi.resetAllMocks()
  resetNextNavigation()
})

afterEach(() => {
  vi.restoreAllMocks()
  vi.unstubAllEnvs()
  vi.unstubAllGlobals()
})
