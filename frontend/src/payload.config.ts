import path from 'path'
import { fileURLToPath } from 'url'

import { postgresAdapter } from '@payloadcms/db-postgres'
import { lexicalEditor } from '@payloadcms/richtext-lexical'
import { buildConfig } from 'payload'
import sharp from 'sharp'

import { Media } from './collections/Media'
import { Pages } from './collections/Pages'
import { Users } from './collections/Users'
import { getPayloadEnv } from './env/payload'
import { Header } from './globals/Header'
import { migrations } from './migrations'

const filename = fileURLToPath(import.meta.url)
const dirname = path.dirname(filename)

const { connectionString, secret } = getPayloadEnv()

export default buildConfig({
  admin: {
    user: Users.slug,
    importMap: {
      baseDir: path.resolve(dirname),
    },
  },
  collections: [Users, Media, Pages],
  editor: lexicalEditor(),
  globals: [Header],
  secret,
  typescript: {
    outputFile: path.resolve(dirname, 'payload-types.ts'),
  },
  db: postgresAdapter({
    pool: {
      connectionString,
    },
    prodMigrations: migrations,
  }),
  sharp,
  plugins: [],
})
