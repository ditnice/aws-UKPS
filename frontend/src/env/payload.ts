import { z } from 'zod'

export const nonBlank = z.string().refine((value) => value.trim().length > 0, 'Must not be blank')

// Reports field names and reasons only, so that secret values never reach the logs.
export function formatEnvIssues(
  issues: readonly { path?: readonly unknown[]; message: string }[],
): string {
  return issues.map(({ path, message }) => `${path?.join('.')}: ${message}`).join('\n')
}

const postgresUrl = z.string().refine((value) => {
  const url = URL.parse(value)
  return (
    url &&
    /^postgres(?:ql)?:\/\//i.test(value) &&
    (url.hostname.length > 0 || url.pathname.length > 1 || Boolean(url.searchParams.get('host')))
  )
}, 'Must be a PostgreSQL connection URL')

// The host is interpolated into a connection URL, so URL syntax could displace its TLS settings.
const databaseHost = /^(?:[a-z0-9._-]+|\[[0-9a-f:.]+\])$/i

// Formats are checked in refineSplitDatabase, as these fields are ignored when DATABASE_URL is set.
const splitDatabaseFields = {
  DATABASE_HOST: z.string().optional(),
  DATABASE_NAME: z.string().optional(),
  DATABASE_PASSWORD: z.string().optional(),
  DATABASE_PORT: z.string().optional(),
  DATABASE_USERNAME: z.string().optional(),
}

export const payloadEnvSchema = {
  PAYLOAD_SECRET: nonBlank,
  DATABASE_URL: postgresUrl.optional(),
  ...splitDatabaseFields,
}

export function readPayloadEnv(source: Record<string, string | undefined>) {
  return {
    PAYLOAD_SECRET: source.PAYLOAD_SECRET || undefined,
    DATABASE_URL: source.DATABASE_URL || undefined,
    DATABASE_HOST: source.DATABASE_HOST,
    DATABASE_NAME: source.DATABASE_NAME,
    DATABASE_PASSWORD: source.DATABASE_PASSWORD,
    DATABASE_PORT: source.DATABASE_PORT,
    DATABASE_USERNAME: source.DATABASE_USERNAME,
  }
}

type PayloadEnv = z.infer<z.ZodObject<typeof payloadEnvSchema>>

function refineSplitDatabase(env: Partial<PayloadEnv>, ctx: z.RefinementCtx) {
  if (env.DATABASE_URL) return

  const host = env.DATABASE_HOST
  if (host?.trim() && !databaseHost.test(host)) {
    ctx.addIssue({
      code: 'custom',
      path: ['DATABASE_HOST'],
      message: 'Must be a hostname or IP address',
    })
  }

  const port = env.DATABASE_PORT
  if (port?.trim() && (!/^\d+$/.test(port) || Number(port) < 1 || Number(port) > 65535)) {
    ctx.addIssue({
      code: 'custom',
      path: ['DATABASE_PORT'],
      message: 'Must be an integer from 1 to 65535',
    })
  }
}

export function requirePayloadDatabase(env: PayloadEnv, ctx: z.RefinementCtx) {
  if (env.DATABASE_URL) return

  for (const key of Object.keys(splitDatabaseFields) as (keyof typeof splitDatabaseFields)[]) {
    if (!env[key]?.trim()) {
      ctx.addIssue({
        code: 'custom',
        path: [key],
        message: 'Required when DATABASE_URL is not set',
      })
    }
  }

  refineSplitDatabase(env, ctx)
}

// Payload's CLI loads this outside Next.js. Builds and code generation may omit credentials.
const toolingSchema = z.object(payloadEnvSchema).partial().superRefine(refineSplitDatabase)

export function getPayloadEnv(source: Record<string, string | undefined> = process.env) {
  const result = toolingSchema.safeParse(readPayloadEnv(source))
  if (!result.success) {
    throw new Error(`Invalid Payload environment:\n${formatEnvIssues(result.error.issues)}`)
  }

  const {
    DATABASE_URL,
    DATABASE_HOST,
    DATABASE_NAME,
    DATABASE_PASSWORD,
    DATABASE_PORT,
    DATABASE_USERNAME,
    PAYLOAD_SECRET,
  } = result.data
  let connectionString = DATABASE_URL ?? ''
  if (
    !connectionString &&
    DATABASE_HOST &&
    DATABASE_NAME &&
    DATABASE_PASSWORD &&
    DATABASE_PORT &&
    DATABASE_USERNAME
  ) {
    const username = encodeURIComponent(DATABASE_USERNAME)
    const password = encodeURIComponent(DATABASE_PASSWORD)
    const database = encodeURIComponent(DATABASE_NAME)
    connectionString = `postgres://${username}:${password}@${DATABASE_HOST}:${DATABASE_PORT}/${database}?sslmode=verify-full`
  }
  return { connectionString, secret: PAYLOAD_SECRET ?? '' }
}
