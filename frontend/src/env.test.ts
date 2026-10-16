import { beforeEach, describe, expect, it, vi } from 'vitest'

const valid = {
  BACKEND_API_BASE_URL: 'https://api.example.test/base/path',
  PAYLOAD_SECRET: ' test secret with spaces ',
  DATABASE_URL: 'postgresql://user:password@localhost:5432/db',
  COGNITO_ISSUER: 'https://issuer.example.test/pool',
  COGNITO_CLIENT_ID: 'test-client',
  NEXT_PUBLIC_QA_SUPPORT_EMAIL: 'support@example.test',
}
const fields = {
  DATABASE_HOST: 'localhost',
  DATABASE_NAME: 'test /database',
  DATABASE_PASSWORD: ' p@ss:/?#% word ',
  DATABASE_PORT: '5432',
  DATABASE_USERNAME: 'user@domain',
}
const keys = [
  ...Object.keys(valid),
  ...Object.keys(fields),
  'AUTHENTICATION_MODE',
  'BACKEND_API_TIMEOUT_MS',
  'FRONTEND_PUBLIC_ORIGIN',
]

async function load(overrides: Record<string, string | undefined> = {}) {
  vi.resetModules()
  for (const key of keys) vi.stubEnv(key, undefined)
  for (const [key, value] of Object.entries({ ...valid, ...overrides })) vi.stubEnv(key, value)
  return import('./env')
}

beforeEach(() => vi.resetModules())

describe('environment validation', () => {
  it('preserves secret bytes, backend base paths and default timeout', async () => {
    const { env, getDatabaseConnectionString } = await load()
    expect(env.PAYLOAD_SECRET).toBe(valid.PAYLOAD_SECRET)
    expect(env.BACKEND_API_BASE_URL).toBe(valid.BACKEND_API_BASE_URL)
    expect(env.BACKEND_API_TIMEOUT_MS).toBe(15_000)
    expect(env.FRONTEND_PUBLIC_ORIGIN).toBeUndefined()
    expect(env.AUTHENTICATION_MODE).toBeUndefined()
    expect(getDatabaseConnectionString()).toBe(valid.DATABASE_URL)
  })

  it.each(Object.keys(valid))('rejects missing %s', async (key) => {
    await expect(load({ [key]: undefined })).rejects.toThrow('Invalid environment variables')
  })

  it.each([
    ['BACKEND_API_BASE_URL', 'ftp://example.test'],
    ['BACKEND_API_BASE_URL', 'https://user:password@example.test/base'],
    ['BACKEND_API_BASE_URL', 'not a URL'],
    ['BACKEND_API_BASE_URL', ' https://example.test'],
    ['PAYLOAD_SECRET', ' \t\n'],
    ['DATABASE_URL', 'https://example.test/db'],
    ['DATABASE_URL', 'postgres://localhost'],
    ['DATABASE_URL', 'postgres://localhost:65536/db'],
    ['DATABASE_URL', 'postgres://localhost:0/db'],
    ['DATABASE_URL', ''],
    ['AUTHENTICATION_MODE', 'PROD'],
    ['AUTHENTICATION_MODE', 'dev'],
    ['AUTHENTICATION_MODE', ''],
    ['COGNITO_ISSUER', 'file:///issuer'],
    ['COGNITO_CLIENT_ID', '   '],
    ['NEXT_PUBLIC_QA_SUPPORT_EMAIL', 'not-email'],
    ['NEXT_PUBLIC_QA_SUPPORT_EMAIL', ''],
    ['NEXT_PUBLIC_QA_SUPPORT_EMAIL', ' support@example.test '],
  ])('rejects malformed %s (%s)', async (key, value) => {
    await expect(load({ [key]: value })).rejects.toThrow(key)
  })

  it.each([
    '0',
    '-1',
    '1.5',
    'abc',
    '',
    ' ',
    '1e3',
    '2147483648',
    '4294967295',
    '4294967296',
    '9007199254740991',
    '9007199254740992',
    'Infinity',
  ])('rejects timeout %s', async (value) => {
    await expect(load({ BACKEND_API_TIMEOUT_MS: value })).rejects.toThrow('BACKEND_API_TIMEOUT_MS')
  })

  it.each(['1', '2500', '2147483647'])('accepts Node-compatible timeout %s', async (value) => {
    const timeout = (await load({ BACKEND_API_TIMEOUT_MS: value })).env.BACKEND_API_TIMEOUT_MS
    expect(timeout).toBe(Number(value))
  })

  it('uses the maximum timeout without warning or clamping it to 1 ms', async () => {
    const { env } = await load({ BACKEND_API_TIMEOUT_MS: '2147483647' })
    const warning = vi.spyOn(process, 'emitWarning')
    const signal = AbortSignal.timeout(env.BACKEND_API_TIMEOUT_MS)
    await new Promise((resolve) => setTimeout(resolve, 10))
    expect(signal.aborted).toBe(false)
    expect(warning).not.toHaveBeenCalled()
  })

  it.each([
    'https://frontend.test/path',
    'https://frontend.test?x=1',
    'https://frontend.test/#hash',
    'https://frontend.test?',
    'not-url',
    'file:///path',
    'https://user:pass@frontend.test',
    '',
  ])('rejects non-origin %s', async (value) => {
    await expect(load({ FRONTEND_PUBLIC_ORIGIN: value })).rejects.toThrow('FRONTEND_PUBLIC_ORIGIN')
  })

  it.each(['https://frontend.test', 'https://frontend.test/', 'http://localhost:3000'])(
    'accepts and normalizes origin %s',
    async (value) => {
      expect((await load({ FRONTEND_PUBLIC_ORIGIN: value })).env.FRONTEND_PUBLIC_ORIGIN).toBe(
        new URL(value).origin,
      )
    },
  )

  it('allows absent Cognito only in DEV, but validates supplied values', async () => {
    await expect(
      load({ AUTHENTICATION_MODE: 'DEV', COGNITO_ISSUER: undefined, COGNITO_CLIENT_ID: undefined }),
    ).resolves.toBeDefined()
    await expect(load({ AUTHENTICATION_MODE: 'DEV', COGNITO_ISSUER: 'invalid' })).rejects.toThrow(
      'COGNITO_ISSUER',
    )
  })

  it('encodes separate database credentials and name without modifying their bytes', async () => {
    const { env, getDatabaseConnectionString } = await load({ DATABASE_URL: undefined, ...fields })
    expect(env.DATABASE_PASSWORD).toBe(fields.DATABASE_PASSWORD)
    const url = new URL(getDatabaseConnectionString())
    expect(decodeURIComponent(url.username)).toBe(fields.DATABASE_USERNAME)
    expect(decodeURIComponent(url.password)).toBe(fields.DATABASE_PASSWORD)
    expect(decodeURIComponent(url.pathname.slice(1))).toBe(fields.DATABASE_NAME)
    expect(url.searchParams.get('sslmode')).toBe('verify-full')
  })

  it.each(Object.keys(fields))('requires complete separate DB configuration: %s', async (key) => {
    await expect(load({ DATABASE_URL: undefined, ...fields, [key]: undefined })).rejects.toThrow(
      key,
    )
    await expect(load({ DATABASE_URL: undefined, ...fields, [key]: '  ' })).rejects.toThrow(key)
  })

  it.each(['0', '65536', '-1', '1.5', 'Infinity', '1e3'])('rejects DB port %s', async (value) => {
    await expect(
      load({ DATABASE_URL: undefined, ...fields, DATABASE_PORT: value }),
    ).rejects.toThrow('DATABASE_PORT')
  })

  it.each(['1', '65535'])('accepts DB port boundary %s', async (value) => {
    await expect(
      load({ DATABASE_URL: undefined, ...fields, DATABASE_PORT: value }),
    ).resolves.toBeDefined()
  })

  it.each(['host/db', 'user@host', 'host?sslmode=disable', 'host:1234', ' host'])(
    'rejects DB host fragments %s',
    async (value) => {
      await expect(
        load({ DATABASE_URL: undefined, ...fields, DATABASE_HOST: value }),
      ).rejects.toThrow('DATABASE_HOST')
    },
  )

  it('gives a supplied URL precedence over invalid or incomplete separate fields', async () => {
    const { getDatabaseConnectionString } = await load({
      DATABASE_PORT: 'invalid',
      DATABASE_PASSWORD: ' ',
    })
    expect(getDatabaseConnectionString()).toBe(valid.DATABASE_URL)
    await expect(load({ DATABASE_URL: 'invalid', ...fields })).rejects.toThrow('DATABASE_URL')
  })

  it('never includes values in validation errors or console output', async () => {
    const errorSpy = vi.spyOn(console, 'error')
    const secret = 'secret-marker-that-must-not-appear'
    await expect(
      load({ DATABASE_URL: `https://user:${secret}@host/db`, AUTHENTICATION_MODE: secret }),
    ).rejects.toThrow(/^Invalid environment variables: DATABASE_URL, AUTHENTICATION_MODE$/)
    expect(errorSpy).not.toHaveBeenCalled()
  })

  it('validates only public configuration in browsers and prohibits server access', async () => {
    vi.stubGlobal('window', {})
    const { env, getDatabaseConnectionString } = await load({
      BACKEND_API_BASE_URL: undefined,
      PAYLOAD_SECRET: undefined,
      DATABASE_URL: undefined,
      COGNITO_ISSUER: undefined,
      COGNITO_CLIENT_ID: undefined,
    })
    expect(env.NEXT_PUBLIC_QA_SUPPORT_EMAIL).toBe(valid.NEXT_PUBLIC_QA_SUPPORT_EMAIL)
    expect(() => env.PAYLOAD_SECRET).toThrow('not available in the browser')
    expect(() => env.BACKEND_API_BASE_URL).toThrow('not available in the browser')
    expect(getDatabaseConnectionString).toThrow('not available in the browser')
    await expect(load({ NEXT_PUBLIC_QA_SUPPORT_EMAIL: undefined })).rejects.toThrow(
      'NEXT_PUBLIC_QA_SUPPORT_EMAIL',
    )
  })
})
