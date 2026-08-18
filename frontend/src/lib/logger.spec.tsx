import { Writable } from 'node:stream'

import pino from 'pino'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('server-only', () => ({}))

function captureStream() {
  const lines: string[] = []
  const stream = new Writable({
    write(chunk, _encoding, callback) {
      lines.push(chunk.toString())
      callback()
    },
  })
  return { lines, stream }
}

async function loadLogger() {
  vi.resetModules()
  return import('./logger')
}

afterEach(() => {
  vi.unstubAllEnvs()
})

describe('logger', () => {
  it('defaults to info level in production', async () => {
    vi.stubEnv('NODE_ENV', 'production')
    vi.stubEnv('LOG_LEVEL', '')
    delete process.env.LOG_LEVEL

    const { logger } = await loadLogger()

    expect(logger.level).toBe('info')
  })

  it('defaults to debug level outside production', async () => {
    vi.stubEnv('NODE_ENV', 'test')
    delete process.env.LOG_LEVEL

    const { logger } = await loadLogger()

    expect(logger.level).toBe('debug')
  })

  it('lets LOG_LEVEL override the environment default', async () => {
    vi.stubEnv('NODE_ENV', 'production')
    vi.stubEnv('LOG_LEVEL', 'trace')

    const { logger } = await loadLogger()

    expect(logger.level).toBe('trace')
  })

  it('redacts sensitive fields and leaves other fields intact', () => {
    const { lines, stream } = captureStream()
    const testLogger = pino(
      {
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
      },
      stream,
    )

    testLogger.warn({
      access_token: 'super-secret-access-token',
      cookie: 'a=b; refresh_token=super-secret-refresh-token',
      headers: { authorization: 'Bearer super-secret-bearer-token' },
      method: 'POST',
    })

    const [line] = lines
    expect(line).toContain('[REDACTED]')
    expect(line).not.toContain('super-secret-access-token')
    expect(line).not.toContain('super-secret-bearer-token')
    expect(line).toContain('"method":"POST"')
  })
})
