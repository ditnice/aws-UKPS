// @vitest-environment node

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

const managedVariables = [
  'AUTHENTICATION_MODE',
  'BACKEND_API_BASE_URL',
  'BACKEND_API_TIMEOUT_MS',
  'COGNITO_CLIENT_ID',
  'COGNITO_ISSUER',
  'DATABASE_HOST',
  'DATABASE_NAME',
  'DATABASE_PASSWORD',
  'DATABASE_PORT',
  'DATABASE_URL',
  'DATABASE_USERNAME',
  'FRONTEND_PUBLIC_ORIGIN',
  'PAYLOAD_SECRET',
  'NEXT_PHASE',
  'SKIP_ENV_VALIDATION',
] as const

async function loadEnv() {
  vi.resetModules()
  return (await import('./server')).env
}

beforeEach(() => {
  for (const name of managedVariables) vi.stubEnv(name, undefined)

  vi.stubEnv('AUTHENTICATION_MODE', 'DEV')
  vi.stubEnv('BACKEND_API_BASE_URL', 'https://api.example.test')
  vi.stubEnv('DATABASE_URL', 'postgres://user:password@database.example:5432/ukps')
  vi.stubEnv('PAYLOAD_SECRET', 'test-payload-secret-at-least-32-characters')
  vi.spyOn(console, 'error').mockImplementation(() => undefined)
})

afterEach(() => {
  vi.restoreAllMocks()
  vi.unstubAllEnvs()
})

describe('server environment', () => {
  it('validates a database URL and applies the timeout default', async () => {
    const env = await loadEnv()

    expect(env.DATABASE_URL).toBe('postgres://user:password@database.example:5432/ukps')
    expect(env.BACKEND_API_TIMEOUT_MS).toBe(15_000)
  })

  it('accepts a structured PostgreSQL URL with query options', async () => {
    vi.stubEnv(
      'DATABASE_URL',
      'postgresql://user:password@database.example:5432/ukps?sslmode=verify-full',
    )

    await expect(loadEnv()).resolves.toMatchObject({
      DATABASE_URL: 'postgresql://user:password@database.example:5432/ukps?sslmode=verify-full',
    })
  })

  it.each(['postgres:ukps', 'postgresql:///ukps', 'postgresql://database.example'])(
    'rejects a PostgreSQL URL without TCP host and database structure: %s',
    async (databaseUrl) => {
      vi.stubEnv('DATABASE_URL', databaseUrl)

      await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
    },
  )

  it('validates split database configuration and parses the timeout', async () => {
    vi.stubEnv('DATABASE_URL', undefined)
    vi.stubEnv('DATABASE_HOST', 'database.example')
    vi.stubEnv('DATABASE_NAME', 'ukps')
    vi.stubEnv('DATABASE_PASSWORD', 'password')
    vi.stubEnv('DATABASE_PORT', '5432')
    vi.stubEnv('DATABASE_USERNAME', 'user')
    vi.stubEnv('BACKEND_API_TIMEOUT_MS', '60000')

    const env = await loadEnv()

    expect(env.DATABASE_PORT).toBe(5432)
    expect(env.BACKEND_API_TIMEOUT_MS).toBe(60_000)
  })

  it.each(['database.example', '127.0.0.1'])(
    'accepts a DNS or IPv4 database host: %s',
    async (host) => {
      vi.stubEnv('DATABASE_URL', undefined)
      vi.stubEnv('DATABASE_HOST', host)
      vi.stubEnv('DATABASE_NAME', 'ukps')
      vi.stubEnv('DATABASE_PASSWORD', 'password')
      vi.stubEnv('DATABASE_PORT', '5432')
      vi.stubEnv('DATABASE_USERNAME', 'user')

      await expect(loadEnv()).resolves.toMatchObject({ DATABASE_HOST: host })
    },
  )

  it.each(['https://database.example', 'database.example:5432', 'database/path', '::1'])(
    'rejects an invalid split database host: %s',
    async (host) => {
      vi.stubEnv('DATABASE_URL', undefined)
      vi.stubEnv('DATABASE_HOST', host)
      vi.stubEnv('DATABASE_NAME', 'ukps')
      vi.stubEnv('DATABASE_PASSWORD', 'password')
      vi.stubEnv('DATABASE_PORT', '5432')
      vi.stubEnv('DATABASE_USERNAME', 'user')

      await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
    },
  )

  it('preserves an opaque database password', async () => {
    vi.stubEnv('DATABASE_URL', undefined)
    vi.stubEnv('DATABASE_HOST', 'database.example')
    vi.stubEnv('DATABASE_NAME', 'ukps')
    vi.stubEnv('DATABASE_PASSWORD', ' password with spaces ')
    vi.stubEnv('DATABASE_PORT', '5432')
    vi.stubEnv('DATABASE_USERNAME', 'user')

    const env = await loadEnv()

    expect(env.DATABASE_PASSWORD).toBe(' password with spaces ')
  })

  it('rejects incomplete split database configuration', async () => {
    vi.stubEnv('DATABASE_URL', undefined)
    vi.stubEnv('DATABASE_HOST', 'database.example')

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it('rejects stale invalid split configuration when DATABASE_URL is present', async () => {
    vi.stubEnv('DATABASE_PORT', 'not-a-port')

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it('requires Cognito configuration outside development authentication mode', async () => {
    vi.stubEnv('AUTHENTICATION_MODE', undefined)

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it('accepts Cognito configuration outside development authentication mode', async () => {
    vi.stubEnv('AUTHENTICATION_MODE', undefined)
    vi.stubEnv('COGNITO_CLIENT_ID', 'clientid123')
    vi.stubEnv('COGNITO_ISSUER', 'https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test123')

    await expect(loadEnv()).resolves.toMatchObject({
      COGNITO_CLIENT_ID: 'clientid123',
      COGNITO_ISSUER: 'https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test123',
    })
  })

  it.each([
    'http://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test123',
    'https://identity.example.test/eu-west-2_test123',
    'https://cognito-idp.eu-west-2.amazonaws.com/us-east-1_test123',
    'https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test123/',
    'https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test123?region=test',
  ])('rejects an unsafe or non-canonical Cognito issuer: %s', async (issuer) => {
    vi.stubEnv('AUTHENTICATION_MODE', undefined)
    vi.stubEnv('COGNITO_CLIENT_ID', 'clientid123')
    vi.stubEnv('COGNITO_ISSUER', issuer)

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it('rejects a non-alphanumeric Cognito client ID', async () => {
    vi.stubEnv('AUTHENTICATION_MODE', undefined)
    vi.stubEnv('COGNITO_CLIENT_ID', 'client-id')
    vi.stubEnv('COGNITO_ISSUER', 'https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test123')

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it('rejects development authentication mode in production', async () => {
    vi.stubEnv('NODE_ENV', 'production')

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it.each([
    'file:///etc/passwd',
    'http://api.example.test',
    ' https://api.example.test',
    'https://api.example.test/',
    'https://api.example.test?region=test',
    'https://api.example.test#users',
  ])('rejects an invalid or non-canonical backend URL: %s', async (baseUrl) => {
    vi.stubEnv('BACKEND_API_BASE_URL', baseUrl)

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it.each([
    'https://frontend.example.test',
    'http://localhost:3000',
    'http://app.localhost:3000',
    'http://127.0.0.1:3000',
    'http://[::1]:3000',
  ])('accepts a secure or loopback frontend origin: %s', async (origin) => {
    vi.stubEnv('FRONTEND_PUBLIC_ORIGIN', origin)

    await expect(loadEnv()).resolves.toMatchObject({ FRONTEND_PUBLIC_ORIGIN: origin })
  })

  it.each([
    'http://frontend.example.test',
    'https://frontend.example.test/',
    'https://frontend.example.test/path',
  ])('rejects an insecure or non-canonical frontend origin: %s', async (origin) => {
    vi.stubEnv('FRONTEND_PUBLIC_ORIGIN', origin)

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it('rejects a timeout above five minutes', async () => {
    vi.stubEnv('BACKEND_API_TIMEOUT_MS', '300001')

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it.each(['short', '                                '])(
    'rejects a weak or blank Payload secret',
    async (secret) => {
      vi.stubEnv('PAYLOAD_SECRET', secret)

      await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
    },
  )

  it.each(['DATABASE_HOST', 'DATABASE_NAME', 'DATABASE_USERNAME'] as const)(
    'rejects a blank %s',
    async (name) => {
      vi.stubEnv('DATABASE_URL', undefined)
      vi.stubEnv('DATABASE_HOST', 'database.example')
      vi.stubEnv('DATABASE_NAME', 'ukps')
      vi.stubEnv('DATABASE_PASSWORD', 'password')
      vi.stubEnv('DATABASE_PORT', '5432')
      vi.stubEnv('DATABASE_USERNAME', 'user')
      vi.stubEnv(name, '   ')

      await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
    },
  )

  it.each(['DATABASE_NAME', 'DATABASE_USERNAME'] as const)(
    'rejects leading or trailing whitespace in %s',
    async (name) => {
      vi.stubEnv('DATABASE_URL', undefined)
      vi.stubEnv('DATABASE_HOST', 'database.example')
      vi.stubEnv('DATABASE_NAME', 'ukps')
      vi.stubEnv('DATABASE_PASSWORD', 'password')
      vi.stubEnv('DATABASE_PORT', '5432')
      vi.stubEnv('DATABASE_USERNAME', 'user')
      vi.stubEnv(name, ' value ')

      await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
    },
  )

  it('does not honor SKIP_ENV_VALIDATION outside a production build', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', undefined)
    vi.stubEnv('SKIP_ENV_VALIDATION', '1')

    await expect(loadEnv()).rejects.toThrow('Invalid environment variables')
  })

  it('honors SKIP_ENV_VALIDATION during a production build', async () => {
    vi.stubEnv('AUTHENTICATION_MODE', undefined)
    vi.stubEnv('BACKEND_API_BASE_URL', undefined)
    vi.stubEnv('DATABASE_URL', undefined)
    vi.stubEnv('NEXT_PHASE', 'phase-production-build')
    vi.stubEnv('PAYLOAD_SECRET', undefined)
    vi.stubEnv('SKIP_ENV_VALIDATION', '1')

    await expect(loadEnv()).resolves.toBeDefined()
  })
})
