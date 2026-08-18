import pino from 'pino'
import 'server-only'

const level = process.env.LOG_LEVEL ?? (process.env.NODE_ENV === 'production' ? 'info' : 'debug')

export const logger = pino({
  base: {
    env: process.env.NODE_ENV,
    service: 'ukps-frontend',
  },
  level,
  redact: {
    censor: '[REDACTED]',
    paths: [
      'cookie',
      'headers.cookie',
      'headers.authorization',
      'headers["x-csrf-token"]',
      'access_token',
      'refresh_token',
      'csrf_token',
      '*.access_token',
      '*.refresh_token',
      '*.csrf_token',
    ],
  },
  timestamp: pino.stdTimeFunctions.isoTime,
})
