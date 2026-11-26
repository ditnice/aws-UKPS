import { describe, expect, it } from 'vitest'

import { getPayloadEnv } from './payload'

const splitDatabase = {
  DATABASE_HOST: 'db.example.test',
  DATABASE_NAME: 'example/name',
  DATABASE_PASSWORD: 'a@b:c',
  DATABASE_PORT: '5432',
  DATABASE_USERNAME: 'user+name',
}

describe('getPayloadEnv', () => {
  it('allows code generation and builds without runtime credentials', () => {
    expect(getPayloadEnv({})).toEqual({ connectionString: '', secret: '' })
  })

  it('treats an empty secret as unset', () => {
    expect(getPayloadEnv({ PAYLOAD_SECRET: '' }).secret).toBe('')
  })

  it('reports why a setting is invalid without including its value', () => {
    expect(() => getPayloadEnv({ DATABASE_URL: 'mysql://user:sensitive-value@db' })).toThrow(
      /^Invalid Payload environment:\nDATABASE_URL: Must be a PostgreSQL connection URL$/,
    )
  })

  it('accepts a PostgreSQL URL scheme in any case', () => {
    const url = 'PostgreSQL://db.example.test/other'
    expect(getPayloadEnv({ DATABASE_URL: url }).connectionString).toBe(url)
  })

  it('prefers DATABASE_URL over split settings', () => {
    expect(
      getPayloadEnv({
        ...splitDatabase,
        DATABASE_URL: 'postgres://db.example.test/other',
        PAYLOAD_SECRET: 'test-secret',
      }),
    ).toEqual({ connectionString: 'postgres://db.example.test/other', secret: 'test-secret' })
  })

  it.each(['db/other?sslmode=disable#', 'user@db', 'db:5433', 'db name', '[::1]/other'])(
    'rejects database host %s',
    (host) => {
      expect(() => getPayloadEnv({ ...splitDatabase, DATABASE_HOST: host })).toThrow(
        'DATABASE_HOST: Must be a hostname or IP address',
      )
    },
  )

  it.each(['localhost', 'db_1.cluster-abc.eu-west-2.rds.amazonaws.com', '10.0.0.1', '[::1]'])(
    'accepts database host %s',
    (host) => {
      const { connectionString } = getPayloadEnv({ ...splitDatabase, DATABASE_HOST: host })
      expect(new URL(connectionString).searchParams.get('sslmode')).toBe('verify-full')
    },
  )

  it('ignores an invalid split database setting when DATABASE_URL is set', () => {
    const url = 'postgres://db.example.test/other'
    expect(
      getPayloadEnv({ DATABASE_HOST: 'a/b', DATABASE_PORT: 'x', DATABASE_URL: url })
        .connectionString,
    ).toBe(url)
  })

  it('escapes split database fields and preserves verify-full TLS', () => {
    expect(getPayloadEnv(splitDatabase).connectionString).toBe(
      'postgres://user%2Bname:a%40b%3Ac@db.example.test:5432/example%2Fname?sslmode=verify-full',
    )
  })
})
