import './vitest.setup'

// Integration tests must explicitly opt into a disposable database, never a developer's DB.
const databaseUrl = process.env.PAYLOAD_TEST_DATABASE_URL
if (!databaseUrl) {
  throw new Error('Set PAYLOAD_TEST_DATABASE_URL to a disposable PostgreSQL test database.')
}
process.env.DATABASE_URL = databaseUrl
