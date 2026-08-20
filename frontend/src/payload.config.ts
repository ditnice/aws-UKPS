import path from 'path'
import { fileURLToPath } from 'url'

import { postgresAdapter } from '@payloadcms/db-postgres'
import { lexicalEditor } from '@payloadcms/richtext-lexical'
import { buildConfig } from 'payload'
import sharp from 'sharp'

import { Media } from './collections/Media'
import { Pages } from './collections/Pages'
import { Users } from './collections/Users'

const filename = fileURLToPath(import.meta.url)
const dirname = path.dirname(filename)

function getDatabaseConnectionString(): string {
  if (process.env.DATABASE_URL) {
    return process.env.DATABASE_URL
  }

  const { DATABASE_HOST, DATABASE_NAME, DATABASE_PASSWORD, DATABASE_PORT, DATABASE_USERNAME } =
    process.env

  if (DATABASE_HOST && DATABASE_NAME && DATABASE_PASSWORD && DATABASE_PORT && DATABASE_USERNAME) {
    const username = encodeURIComponent(DATABASE_USERNAME)
    const password = encodeURIComponent(DATABASE_PASSWORD)
    const databaseName = encodeURIComponent(DATABASE_NAME)

    return `postgres://${username}:${password}@${DATABASE_HOST}:${DATABASE_PORT}/${databaseName}?sslmode=require`
  }

  return ''
}

export default buildConfig({
  admin: {
    user: Users.slug,
    importMap: {
      baseDir: path.resolve(dirname),
    },
  },
  collections: [Users, Media, Pages],
  editor: lexicalEditor(),
  secret: process.env.PAYLOAD_SECRET || '',
  typescript: {
    outputFile: path.resolve(dirname, 'payload-types.ts'),
  },
  db: postgresAdapter({
    pool: {
      connectionString: getDatabaseConnectionString(),
    },
  }),
  sharp,
  plugins: [],
})
