import { createEnv } from '@t3-oss/env-nextjs'
import * as z from 'zod'

const urlInput = z
  .string()
  .refine((value) => value === value.trim(), 'Must not contain surrounding whitespace')

const url = urlInput.pipe(z.url())

const httpUrl = url.refine((value) => {
  const parsedUrl = new URL(value)
  return (
    ['http:', 'https:'].includes(parsedUrl.protocol) && !parsedUrl.username && !parsedUrl.password
  )
}, 'Must be an HTTP(S) URL without embedded credentials')

const cleanHttpUrl = httpUrl.refine((value) => {
  const parsedUrl = new URL(value)
  return !value.endsWith('/') && !parsedUrl.search && !parsedUrl.hash
}, 'Must not contain a trailing slash, a query, or a fragment')

const noEdgeWhitespace = z
  .string()
  .min(1)
  .refine((value) => value === value.trim(), {
    message: 'Must not contain leading or trailing whitespace',
  })

const positiveInteger = z
  .string()
  .regex(/^\d+$/, 'Must be a positive integer')
  .transform(Number)
  .pipe(z.number().int().positive())

const databasePort = positiveInteger.pipe(z.number().max(65_535))
const backendApiTimeout = positiveInteger.pipe(z.number().max(300_000))
const databaseHost = z.union([z.hostname(), z.ipv4()])
const postgresUrl = url.refine((value) => {
  const parsedUrl = new URL(value)
  return (
    ['postgres:', 'postgresql:'].includes(parsedUrl.protocol) &&
    Boolean(parsedUrl.hostname) &&
    Boolean(parsedUrl.pathname) &&
    parsedUrl.pathname !== '/'
  )
}, 'Must be a PostgreSQL URL with a hostname and database path')
const originUrl = urlInput
  .refine((value) => {
    try {
      return new URL(value).origin === value
    } catch {
      return false
    }
  }, 'Must contain only a canonical HTTP(S) origin')
  .pipe(httpUrl)
const backendApiBaseUrl = cleanHttpUrl.refine((value) => new URL(value).protocol === 'https:', {
  message: 'Must use HTTPS',
})
const frontendPublicOrigin = originUrl.refine((value) => {
  const parsedUrl = new URL(value)
  if (parsedUrl.protocol === 'https:') return true
  if (parsedUrl.protocol !== 'http:') return false

  return (
    parsedUrl.hostname === 'localhost' ||
    parsedUrl.hostname.endsWith('.localhost') ||
    parsedUrl.hostname === '[::1]' ||
    parsedUrl.hostname.startsWith('127.')
  )
}, 'Must use HTTPS unless the hostname is localhost or a loopback address')
const cognitoClientId = z.string().regex(/^[A-Za-z0-9]+$/, {
  message: 'Must be an alphanumeric Cognito app-client ID',
})
const cognitoIssuer = cleanHttpUrl.refine((value) => {
  const parsedUrl = new URL(value)
  const hostnameMatch = parsedUrl.hostname.match(
    /^cognito-idp\.([a-z0-9-]+)\.amazonaws\.com(?:\.cn)?$/,
  )
  if (parsedUrl.protocol !== 'https:' || !hostnameMatch) return false

  const region = hostnameMatch[1]
  return parsedUrl.pathname.match(/^\/([a-z0-9-]+)_([A-Za-z0-9]+)$/)?.[1] === region
}, 'Must be a canonical AWS Cognito User Pool issuer')

const skipValidation =
  process.env.SKIP_ENV_VALIDATION === '1' && process.env.NEXT_PHASE === 'phase-production-build'

const payloadSecret = z
  .string()
  .min(32)
  .refine((value) => value.trim().length > 0, {
    message: 'Must not be blank',
  })

export const env = createEnv({
  server: {
    AUTHENTICATION_MODE: z.enum(['DEV']).optional(),
    BACKEND_API_BASE_URL: backendApiBaseUrl,
    BACKEND_API_TIMEOUT_MS: backendApiTimeout.optional().default(15_000),
    COGNITO_CLIENT_ID: cognitoClientId.optional(),
    COGNITO_ISSUER: cognitoIssuer.optional(),
    DATABASE_HOST: databaseHost.optional(),
    DATABASE_NAME: noEdgeWhitespace.optional(),
    DATABASE_PASSWORD: z.string().min(1).optional(),
    DATABASE_PORT: databasePort.optional(),
    DATABASE_URL: postgresUrl.optional(),
    DATABASE_USERNAME: noEdgeWhitespace.optional(),
    FRONTEND_PUBLIC_ORIGIN: frontendPublicOrigin.optional(),
    PAYLOAD_SECRET: payloadSecret,
  },
  createFinalSchema: (shape, isServer) =>
    z.object(shape).superRefine((values, context) => {
      if (!isServer) return

      const databaseParts = [
        values.DATABASE_HOST,
        values.DATABASE_NAME,
        values.DATABASE_PASSWORD,
        values.DATABASE_PORT,
        values.DATABASE_USERNAME,
      ]
      if (!values.DATABASE_URL && databaseParts.some((value) => value === undefined)) {
        context.addIssue({
          code: 'custom',
          message: 'DATABASE_URL or all split database variables must be configured',
          path: ['DATABASE_URL'],
        })
      }

      if (values.AUTHENTICATION_MODE === 'DEV' && process.env.NODE_ENV === 'production') {
        context.addIssue({
          code: 'custom',
          message: 'AUTHENTICATION_MODE cannot be DEV when NODE_ENV is production',
          path: ['AUTHENTICATION_MODE'],
        })
      }

      if (values.AUTHENTICATION_MODE !== 'DEV') {
        for (const name of ['COGNITO_CLIENT_ID', 'COGNITO_ISSUER'] as const) {
          if (!values[name]) {
            context.addIssue({
              code: 'custom',
              message: `${name} is required unless AUTHENTICATION_MODE is DEV`,
              path: [name],
            })
          }
        }
      }
    }),
  emptyStringAsUndefined: true,
  experimental__runtimeEnv: process.env,
  skipValidation,
})
