import { randomUUID } from 'node:crypto'

import { postgresAdapter } from '@payloadcms/db-postgres'
import { buildConfig } from 'payload'

import { Pages } from '@/collections/Pages'
import { Users } from '@/collections/Users'
import { Header } from '@/globals/Header'

const connectionString = process.env.PAYLOAD_TEST_DATABASE_URL
if (!connectionString) {
  throw new Error('Set PAYLOAD_TEST_DATABASE_URL to a disposable PostgreSQL test database.')
}

// Each run owns its schema; never push or delete the application's schema.
export const testSchema = `ukps_access_${randomUUID().replaceAll('-', '')}`

export default buildConfig({
  collections: [Users, Pages],
  globals: [Header],
  secret: 'payload-access-integration-test-secret',
  admin: { importMap: { autoGenerate: false } },
  typescript: { autoGenerate: false },
  db: postgresAdapter({
    pool: { connectionString },
    schemaName: testSchema,
    push: true,
  }),
})
