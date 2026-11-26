import { createEnv } from '@t3-oss/env-nextjs'
import 'server-only'
import { z } from 'zod'

import {
  formatEnvIssues,
  nonBlank,
  payloadEnvSchema,
  readPayloadEnv,
  requirePayloadDatabase,
} from './payload'

const defaultUpstreamTimeoutMs = 15_000
const httpUrl = z.string().refine((value) => {
  const url = URL.parse(value)
  return url && ['http:', 'https:'].includes(url.protocol) && !url.username && !url.password
}, 'Must be an HTTP(S) URL without credentials')

// Request paths are appended to this value, so a query or fragment would swallow them.
const backendBaseUrl = httpUrl.refine(
  (value) => !/[?#]/.test(value),
  'Must not contain a query or fragment',
)

// Must match the token `iss` claim exactly and be safe to append the JWKS path to.
const issuerUrl = httpUrl.refine(
  (value) => !/[?#]|\/$/.test(value),
  'Must not contain a trailing slash, query or fragment',
)

// Keep in step with the qa_support_email validation in infrastructure/environments, so a value
// Terraform accepts cannot stop the server from starting.
const supportEmail = z.string().regex(/^[^@\s]+@[^@\s]+\.[^@\s]+$/, 'Must be an email address')

export function validateServerEnv(source: Record<string, string | undefined> = process.env) {
  return createEnv({
    server: {
      BACKEND_API_BASE_URL: backendBaseUrl,
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
        .pipe(z.number().max(2_147_483_647, 'Must not exceed 2147483647 milliseconds'))
        .default(defaultUpstreamTimeoutMs),
      ROBOTS_NOINDEX: z
        .enum(['true', 'false'], 'Must be true or false')
        .default('false')
        .transform((value) => value === 'true'),
      AUTHENTICATION_MODE: z.enum(['DEV'], 'Must be DEV or unset').optional(),
      COGNITO_ISSUER: issuerUrl.optional(),
      COGNITO_CLIENT_ID: nonBlank.optional(),
      QA_SUPPORT_EMAIL: supportEmail,
      ...payloadEnvSchema,
    },
    // Map values explicitly so an injected environment never falls back to the host process.
    runtimeEnv: {
      BACKEND_API_BASE_URL: source.BACKEND_API_BASE_URL,
      FRONTEND_PUBLIC_ORIGIN: source.FRONTEND_PUBLIC_ORIGIN,
      BACKEND_API_TIMEOUT_MS: source.BACKEND_API_TIMEOUT_MS,
      ROBOTS_NOINDEX: source.ROBOTS_NOINDEX,
      AUTHENTICATION_MODE: source.AUTHENTICATION_MODE,
      COGNITO_ISSUER: source.COGNITO_ISSUER,
      COGNITO_CLIENT_ID: source.COGNITO_CLIENT_ID,
      QA_SUPPORT_EMAIL: source.QA_SUPPORT_EMAIL,
      ...readPayloadEnv(source),
    },
    // An empty value, such as `KEY=` in .env, means unset, so optional settings take defaults.
    emptyStringAsUndefined: true,
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

        requirePayloadDatabase(env, ctx)
      }),
    onValidationError: (issues) => {
      throw new Error(`Invalid frontend environment:\n${formatEnvIssues(issues)}`)
    },
  })
}

let serverEnv: ReturnType<typeof validateServerEnv> | undefined

export function getServerEnv() {
  return (serverEnv ??= validateServerEnv())
}
