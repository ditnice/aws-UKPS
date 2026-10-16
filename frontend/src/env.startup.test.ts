import { spawnSync } from 'node:child_process'
import { mkdtemp, rm, writeFile } from 'node:fs/promises'
import { tmpdir } from 'node:os'
import path from 'node:path'

import { build } from 'esbuild'
import { afterAll, beforeAll, describe, expect, it } from 'vitest'

let directory: string
const localPreflight = path.resolve('scripts/validate-env.mjs')
const valid = {
  BACKEND_API_BASE_URL: 'https://api.example.test/base',
  PAYLOAD_SECRET: 'startup-test-only-secret',
  DATABASE_URL: 'postgres://test:test@127.0.0.1:5432/test',
  COGNITO_ISSUER: 'https://issuer.example.test/pool',
  COGNITO_CLIENT_ID: 'test-client',
}

beforeAll(async () => {
  directory = await mkdtemp(path.join(tmpdir(), 'ukps-env-preflight-'))
  await build({
    entryPoints: ['scripts/start.ts'],
    outfile: path.join(directory, 'start.mjs'),
    bundle: true,
    platform: 'node',
    target: 'node24',
    format: 'esm',
    define: { 'process.env.NEXT_PUBLIC_QA_SUPPORT_EMAIL': JSON.stringify('support@example.test') },
  })
  await writeFile(path.join(directory, 'server.js'), 'console.log("SERVER_STARTED")')
})

afterAll(async () => {
  if (directory) await rm(directory, { recursive: true, force: true })
})

function run(overrides: Record<string, string | undefined> = {}, args: string[] = []) {
  // No inherited application configuration, TS loaders or node_modules resolution.
  return spawnSync(process.execPath, [path.join(directory, 'start.mjs'), ...args], {
    cwd: directory,
    env: { NODE_ENV: 'production', ...valid, ...overrides },
    encoding: 'utf8',
    timeout: 10_000,
  })
}

describe('standalone startup preflight', () => {
  it('validates and then starts the server without dev dependencies', () => {
    const result = run()
    expect(result.status).toBe(0)
    expect(result.stdout).toContain('SERVER_STARTED')
  })

  it('supports a check-only command without importing the server', () => {
    const result = run({}, ['--check-env'])
    expect(result.status).toBe(0)
    expect(result.stdout).not.toContain('SERVER_STARTED')
  })

  it('accepts the maximum usable timeout at standalone startup', () => {
    const result = run({ BACKEND_API_TIMEOUT_MS: '2147483647' })
    expect(result.status).toBe(0)
    expect(result.stdout).toContain('SERVER_STARTED')
    expect(result.stderr).not.toContain('TimeoutOverflowWarning')
  })

  it.each([
    { PAYLOAD_SECRET: undefined },
    { BACKEND_API_BASE_URL: 'invalid' },
    { DATABASE_URL: undefined },
    { COGNITO_ISSUER: undefined },
    { BACKEND_API_TIMEOUT_MS: '0' },
    { BACKEND_API_TIMEOUT_MS: '2147483648' },
  ])('exits before server import for invalid configuration %#', (overrides) => {
    const result = run(overrides)
    expect(result.status).not.toBe(0)
    expect(result.stdout).not.toContain('SERVER_STARTED')
    expect(result.stderr).toContain('Invalid environment variables:')
    expect(result.stderr).not.toContain(valid.PAYLOAD_SECRET)
    expect(result.stderr).not.toContain(valid.DATABASE_URL)
  })

  it('has no environment-variable runtime bypass', () => {
    const result = run({
      PAYLOAD_SECRET: undefined,
      SKIP_ENV_VALIDATION: '1',
      NEXT_PHASE: 'phase-production-build',
    })
    expect(result.status).not.toBe(0)
    expect(result.stdout).not.toContain('SERVER_STARTED')
  })

  it('accepts complete database components at runtime without a URL', () => {
    const result = run({
      DATABASE_URL: undefined,
      DATABASE_HOST: 'localhost',
      DATABASE_PORT: '5432',
      DATABASE_NAME: 'startup_test',
      DATABASE_USERNAME: 'test',
      DATABASE_PASSWORD: 'startup-test-only-password',
    })
    expect(result.status).toBe(0)
    expect(result.stdout).toContain('SERVER_STARTED')
  })

  it('gives the runtime database URL precedence over invalid components', () => {
    const result = run({ DATABASE_PORT: 'invalid', DATABASE_PASSWORD: '' })
    expect(result.status).toBe(0)
    expect(result.stdout).toContain('SERVER_STARTED')
  })

  it('accepts DEV without Cognito and ignores runtime changes to the baked public email', () => {
    const result = run({
      AUTHENTICATION_MODE: 'DEV',
      COGNITO_ISSUER: undefined,
      COGNITO_CLIENT_ID: undefined,
      NEXT_PUBLIC_QA_SUPPORT_EMAIL: 'invalid-runtime-email',
    })
    expect(result.status).toBe(0)
    expect(result.stdout).toContain('SERVER_STARTED')
  })
})

describe('local Next command preflight', () => {
  it.each([{ args: [] }, { args: ['--dev'] }])(
    'validates configuration before launching Next (%j)',
    ({ args }) => {
      const runLocal = (overrides: Record<string, string | undefined> = {}) =>
        spawnSync(process.execPath, [localPreflight, ...args], {
          cwd: directory,
          env: {
            NODE_ENV: args.includes('--dev') ? 'development' : 'production',
            ...valid,
            NEXT_PUBLIC_QA_SUPPORT_EMAIL: 'support@example.test',
            ...overrides,
          },
          encoding: 'utf8',
          timeout: 10_000,
        })
      expect(runLocal().status).toBe(0)
      const invalid = runLocal({ PAYLOAD_SECRET: undefined })
      expect(invalid.status).not.toBe(0)
      expect(invalid.stderr).toContain('Invalid environment variables: PAYLOAD_SECRET')
      expect(invalid.stderr).not.toContain(valid.PAYLOAD_SECRET)
    },
  )
})
