import { createEnv } from '@t3-oss/env-nextjs'
import { z } from 'zod'

// Never trim secrets: spaces may be part of a legitimate credential.
const nonblank = z.string().refine((value) => value.trim().length > 0)
const httpUrl = nonblank.refine((value) => {
  try {
    const url = new URL(value)
    return (
      value === value.trim() &&
      ['http:', 'https:'].includes(url.protocol) &&
      !url.username &&
      !url.password
    )
  } catch {
    return false
  }
})
const positiveInteger = z
  .string()
  .regex(/^\d+$/)
  .transform(Number)
  .pipe(z.number().int().positive().safe())
const databaseUrl = nonblank.refine((value) => {
  try {
    const url = new URL(value)
    return (
      value === value.trim() &&
      ['postgres:', 'postgresql:'].includes(url.protocol) &&
      !!url.hostname &&
      url.pathname.length > 1 &&
      (!url.port || (Number(url.port) >= 1 && Number(url.port) <= 65535))
    )
  } catch {
    return false
  }
})

export const env = createEnv({
  server: {
    BACKEND_API_BASE_URL: httpUrl,
    BACKEND_API_TIMEOUT_MS: positiveInteger
      .pipe(z.number().max(2_147_483_647))
      .optional()
      .default(15_000),
    FRONTEND_PUBLIC_ORIGIN: httpUrl
      .pipe(
        z.string().refine((value) => {
          const url = new URL(value)
          return url.pathname === '/' && !url.search && !url.hash && !/[?#]/.test(value)
        }),
      )
      .transform((value) => new URL(value).origin)
      .optional(),
    PAYLOAD_SECRET: nonblank,
    DATABASE_URL: databaseUrl.optional(),
    // Validated only when DATABASE_URL is absent; the URL takes precedence.
    DATABASE_HOST: z.string().optional(),
    DATABASE_NAME: z.string().optional(),
    DATABASE_PASSWORD: z.string().optional(),
    DATABASE_PORT: z.string().optional(),
    DATABASE_USERNAME: z.string().optional(),
    AUTHENTICATION_MODE: z.literal('DEV').optional(),
    COGNITO_ISSUER: httpUrl.optional(),
    COGNITO_CLIENT_ID: nonblank.optional(),
  },
  client: {
    NEXT_PUBLIC_QA_SUPPORT_EMAIL: z.email(),
  },
  runtimeEnv: {
    BACKEND_API_BASE_URL: process.env.BACKEND_API_BASE_URL,
    BACKEND_API_TIMEOUT_MS: process.env.BACKEND_API_TIMEOUT_MS,
    FRONTEND_PUBLIC_ORIGIN: process.env.FRONTEND_PUBLIC_ORIGIN,
    PAYLOAD_SECRET: process.env.PAYLOAD_SECRET,
    DATABASE_URL: process.env.DATABASE_URL,
    DATABASE_HOST: process.env.DATABASE_HOST,
    DATABASE_NAME: process.env.DATABASE_NAME,
    DATABASE_PASSWORD: process.env.DATABASE_PASSWORD,
    DATABASE_PORT: process.env.DATABASE_PORT,
    DATABASE_USERNAME: process.env.DATABASE_USERNAME,
    AUTHENTICATION_MODE: process.env.AUTHENTICATION_MODE,
    COGNITO_ISSUER: process.env.COGNITO_ISSUER,
    COGNITO_CLIENT_ID: process.env.COGNITO_CLIENT_ID,
    NEXT_PUBLIC_QA_SUPPORT_EMAIL: process.env.NEXT_PUBLIC_QA_SUPPORT_EMAIL,
  },
  createFinalSchema: (shape, isServer) =>
    z.object(shape).superRefine((values, ctx) => {
      if (!isServer) return
      if (values.AUTHENTICATION_MODE !== 'DEV') {
        for (const key of ['COGNITO_ISSUER', 'COGNITO_CLIENT_ID'] as const) {
          if (values[key] === undefined) ctx.addIssue({ code: 'custom', path: [key] })
        }
      }
      if (values.DATABASE_URL !== undefined) return
      const fields = {
        DATABASE_HOST: nonblank.refine((value) => {
          // A hostname (or bracketed IPv6), never a URL or connection-string fragment.
          try {
            const url = new URL(`postgres://${value}:5432/db`)
            return url.hostname === value && !url.username && !url.password
          } catch {
            return false
          }
        }),
        DATABASE_NAME: nonblank,
        DATABASE_PASSWORD: nonblank,
        DATABASE_PORT: positiveInteger.pipe(z.number().max(65535)),
        DATABASE_USERNAME: nonblank,
      }
      for (const key of Object.keys(fields) as (keyof typeof fields)[]) {
        if (!fields[key].safeParse(values[key]).success) {
          ctx.addIssue({ code: 'custom', path: [key] })
        }
      }
    }),
  onValidationError: (issues) => {
    // Zod issues can contain input values. Report names only, never the issues themselves.
    const keys = [...new Set(issues.map((issue) => String(issue.path?.[0] ?? 'configuration')))]
    throw new Error(`Invalid environment variables: ${keys.join(', ')}`)
  },
  onInvalidAccess: () => {
    throw new Error('Server environment variables are not available in the browser')
  },
})

export function getDatabaseConnectionString(): string {
  if (env.DATABASE_URL !== undefined) return env.DATABASE_URL
  return `postgres://${encodeURIComponent(env.DATABASE_USERNAME!)}:${encodeURIComponent(env.DATABASE_PASSWORD!)}@${env.DATABASE_HOST}:${env.DATABASE_PORT}/${encodeURIComponent(env.DATABASE_NAME!)}?sslmode=verify-full`
}
