import { createEnv } from '@t3-oss/env-nextjs'
import 'server-only'
import { z } from 'zod'

const defaultUpstreamTimeoutMs = 15_000
const nonBlank = z.string().refine((value) => value.trim().length > 0, 'Must not be blank')
const httpUrl = z.string().refine((value) => {
  const url = URL.parse(value)
  return url && ['http:', 'https:'].includes(url.protocol) && !url.username && !url.password
}, 'Must be an HTTP(S) URL without credentials')

// Must match the token `iss` claim exactly and be safe to append the JWKS path to.
const issuerUrl = httpUrl.refine(
  (value) => !/[?#]|\/$/.test(value),
  'Must not contain a trailing slash, query or fragment',
)

const postgresUrl = z.string().refine((value) => {
  const url = URL.parse(value)
  return (
    url &&
    /^postgres(?:ql)?:\/\//.test(value) &&
    (url.hostname.length > 0 || url.pathname.length > 1 || Boolean(url.searchParams.get('host')))
  )
}, 'Must be a PostgreSQL connection URL')

// Keep in step with the qa_support_email validation in infrastructure/environments, so a value
// Terraform accepts cannot stop the server from starting.
const supportEmail = z.string().regex(/^[^@\s]+@[^@\s]+\.[^@\s]+$/, 'Must be an email address')

const splitDatabaseFields = [
  'DATABASE_HOST',
  'DATABASE_NAME',
  'DATABASE_PASSWORD',
  'DATABASE_PORT',
  'DATABASE_USERNAME',
] as const

export function validateServerEnv(source: Record<string, string | undefined> = process.env) {
  return createEnv({
    server: {
      BACKEND_API_BASE_URL: httpUrl,
      FRONTEND_PUBLIC_ORIGIN: httpUrl
        .refine(
          (value) => URL.parse(value)?.origin === value,
          'Must be a canonical origin without a trailing slash',
        )
        .optional(),
      BACKEND_API_TIMEOUT_MS: z
        .string()
        .regex(/^0*[1-9]\d*$/, 'Must be a positive integer')
        .transform(Number)
        .default(defaultUpstreamTimeoutMs),
      ROBOTS_NOINDEX: z
        .enum(['true', 'false'], 'Must be true or false')
        .default('false')
        .transform((value) => value === 'true'),
      AUTHENTICATION_MODE: z.enum(['DEV'], 'Must be DEV or unset').optional(),
      COGNITO_ISSUER: issuerUrl.optional(),
      COGNITO_CLIENT_ID: nonBlank.optional(),
      QA_SUPPORT_EMAIL: supportEmail.optional(),
      PAYLOAD_SECRET: nonBlank,
      DATABASE_URL: postgresUrl.optional(),
      DATABASE_HOST: z.string().optional(),
      DATABASE_NAME: z.string().optional(),
      DATABASE_PASSWORD: z.string().optional(),
      DATABASE_PORT: z.string().optional(),
      DATABASE_USERNAME: z.string().optional(),
    },
    // Map values explicitly so an injected environment never falls back to the host process.
    runtimeEnv: {
      BACKEND_API_BASE_URL: source.BACKEND_API_BASE_URL,
      FRONTEND_PUBLIC_ORIGIN: source.FRONTEND_PUBLIC_ORIGIN,
      BACKEND_API_TIMEOUT_MS: source.BACKEND_API_TIMEOUT_MS || undefined,
      ROBOTS_NOINDEX: source.ROBOTS_NOINDEX || undefined,
      AUTHENTICATION_MODE: source.AUTHENTICATION_MODE || undefined,
      COGNITO_ISSUER: source.COGNITO_ISSUER || undefined,
      COGNITO_CLIENT_ID: source.COGNITO_CLIENT_ID || undefined,
      QA_SUPPORT_EMAIL: source.QA_SUPPORT_EMAIL || undefined,
      PAYLOAD_SECRET: source.PAYLOAD_SECRET,
      DATABASE_URL: source.DATABASE_URL || undefined,
      DATABASE_HOST: source.DATABASE_HOST,
      DATABASE_NAME: source.DATABASE_NAME,
      DATABASE_PASSWORD: source.DATABASE_PASSWORD,
      DATABASE_PORT: source.DATABASE_PORT,
      DATABASE_USERNAME: source.DATABASE_USERNAME,
    },
    createFinalSchema: (shape) =>
      z.object(shape).superRefine((env, ctx) => {
        if (env.AUTHENTICATION_MODE !== 'DEV') {
          for (const key of ['COGNITO_ISSUER', 'COGNITO_CLIENT_ID'] as const) {
            if (!env[key]) {
              ctx.addIssue({
                code: 'custom',
                path: [key],
                message: 'Required unless AUTHENTICATION_MODE is DEV',
              })
            }
          }
        }

        if (!env.DATABASE_URL) {
          for (const key of splitDatabaseFields) {
            if (!env[key]?.trim()) {
              ctx.addIssue({
                code: 'custom',
                path: [key],
                message: 'Required when DATABASE_URL is not set',
              })
            }
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
      }),
    onValidationError: (issues) => {
      const details = issues.map(({ path, message }) => `${path?.join('.')}: ${message}`)
      throw new Error(`Invalid frontend environment:\n${details.join('\n')}`)
    },
  })
}
