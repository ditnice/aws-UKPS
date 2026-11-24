import { describe, expect, it, vi } from 'vitest'

import { validateServerEnv } from './server'

vi.mock('server-only', () => ({}))

const composeEnv = {
  AUTHENTICATION_MODE: 'DEV',
  BACKEND_API_BASE_URL: 'http://backend:8080/v1',
  PAYLOAD_SECRET: 'test-secret',
  DATABASE_URL: 'postgres://postgres:postgres@postgres:5432/payload',
}

const ecsEnv = {
  BACKEND_API_BASE_URL: 'https://api.example.test/v1',
  FRONTEND_PUBLIC_ORIGIN: 'https://app.example.test',
  COGNITO_ISSUER: 'https://cognito.example.test/pool',
  COGNITO_CLIENT_ID: 'test-client',
  QA_SUPPORT_EMAIL: 'support@example.test',
  PAYLOAD_SECRET: 'test-secret',
  DATABASE_HOST: 'db.example.test',
  DATABASE_NAME: 'payload',
  DATABASE_PASSWORD: 'test-password',
  DATABASE_PORT: '5432',
  DATABASE_USERNAME: 'postgres',
}

describe('validateServerEnv', () => {
  it('accepts Compose configuration without Cognito settings', () => {
    expect(validateServerEnv(composeEnv)).toMatchObject({
      BACKEND_API_BASE_URL: composeEnv.BACKEND_API_BASE_URL,
      DATABASE_URL: composeEnv.DATABASE_URL,
      FRONTEND_PUBLIC_ORIGIN: undefined,
      BACKEND_API_TIMEOUT_MS: 15_000,
      ROBOTS_NOINDEX: false,
    })
  })

  it('accepts ECS configuration with Cognito and split database settings', () => {
    expect(validateServerEnv(ecsEnv)).toMatchObject(ecsEnv)
  })

  it('requires Cognito unless auth mode is DEV', () => {
    expect(() => validateServerEnv({ ...composeEnv, AUTHENTICATION_MODE: undefined })).toThrow(
      /COGNITO_ISSUER:[^\n]*\nCOGNITO_CLIENT_ID:/,
    )
  })

  it.each([
    ['BACKEND_API_BASE_URL', undefined],
    ['BACKEND_API_BASE_URL', 'ftp://backend'],
    ['FRONTEND_PUBLIC_ORIGIN', ''],
    ['FRONTEND_PUBLIC_ORIGIN', 'https://app.example.test/path'],
    ['COGNITO_ISSUER', 'https://cognito.example.test/pool/'],
    ['COGNITO_ISSUER', 'https://cognito.example.test/pool?query=value'],
    ['COGNITO_ISSUER', 'https://cognito.example.test/pool#fragment'],
    ['AUTHENTICATION_MODE', 'dev'],
    ['ROBOTS_NOINDEX', 'TRUE'],
    ['BACKEND_API_TIMEOUT_MS', '0'],
    ['BACKEND_API_TIMEOUT_MS', '-1'],
    ['BACKEND_API_TIMEOUT_MS', '1.5'],
    ['BACKEND_API_TIMEOUT_MS', '15s'],
    ['PAYLOAD_SECRET', ' '],
    ['DATABASE_URL', 'postgres:invalid'],
    ['QA_SUPPORT_EMAIL', 'not-an-email'],
  ])('rejects invalid %s', (key, value) => {
    expect(() => validateServerEnv({ ...composeEnv, [key]: value })).toThrow(`${key}:`)
  })

  it('requires split database fields if DATABASE_URL is empty', () => {
    expect(() => validateServerEnv({ ...ecsEnv, DATABASE_URL: '', DATABASE_PASSWORD: '' })).toThrow(
      'DATABASE_PASSWORD:',
    )
  })

  it('validates the split database port only when DATABASE_URL is unset', () => {
    expect(() => validateServerEnv({ ...ecsEnv, DATABASE_PORT: '65536' })).toThrow('DATABASE_PORT:')
    expect(validateServerEnv({ ...composeEnv, DATABASE_PORT: 'not-a-port' }).DATABASE_URL).toBe(
      composeEnv.DATABASE_URL,
    )
  })

  it('treats empty optional settings as unset', () => {
    expect(
      validateServerEnv({
        ...ecsEnv,
        AUTHENTICATION_MODE: '',
        BACKEND_API_TIMEOUT_MS: '',
        QA_SUPPORT_EMAIL: '',
        ROBOTS_NOINDEX: '',
      }),
    ).toMatchObject({
      AUTHENTICATION_MODE: undefined,
      BACKEND_API_TIMEOUT_MS: 15_000,
      QA_SUPPORT_EMAIL: undefined,
      ROBOTS_NOINDEX: false,
    })
  })

  it('parses a configured timeout', () => {
    expect(
      validateServerEnv({ ...composeEnv, BACKEND_API_TIMEOUT_MS: '2500' }).BACKEND_API_TIMEOUT_MS,
    ).toBe(2500)
  })

  it.each([
    ['true', true],
    ['false', false],
  ])('parses ROBOTS_NOINDEX=%s', (value, expected) => {
    expect(validateServerEnv({ ...composeEnv, ROBOTS_NOINDEX: value }).ROBOTS_NOINDEX).toBe(
      expected,
    )
  })

  it('accepts any support email that Terraform accepts', () => {
    const email = 'q.@example.test'
    expect(validateServerEnv({ ...composeEnv, QA_SUPPORT_EMAIL: email }).QA_SUPPORT_EMAIL).toBe(
      email,
    )
  })

  it('does not include raw values in validation diagnostics', () => {
    expect(() =>
      validateServerEnv({
        ...composeEnv,
        BACKEND_API_BASE_URL: 'https://user:sensitive-value@backend.test',
      }),
    ).toThrow(
      /^Invalid frontend environment:\nBACKEND_API_BASE_URL: Must be an HTTP\(S\) URL without credentials$/,
    )
  })
})
