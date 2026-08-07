import { afterEach, describe, expect, it, vi } from 'vitest'

import { createServerApiClient } from './server-api'

const mocks = vi.hoisted(() => ({
  cookies: vi.fn(),
  createClient: vi.fn(),
}))

vi.mock('next/headers', () => ({ cookies: mocks.cookies }))
vi.mock('server-only', () => ({}))
vi.mock('./generated/client', () => ({ createClient: mocks.createClient }))

afterEach(() => {
  vi.clearAllMocks()
  vi.unstubAllEnvs()
})

describe('createServerApiClient', () => {
  it('creates a no-store client with only the access token cookie', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', 'https://api.example.test')
    const getCookie = vi.fn((name: string) =>
      name === 'access_token' ? { value: 'access-token-value' } : undefined,
    )
    mocks.cookies.mockResolvedValue({ get: getCookie })
    const client = { request: vi.fn() }
    mocks.createClient.mockReturnValue(client)

    await expect(createServerApiClient()).resolves.toBe(client)
    expect(getCookie).toHaveBeenCalledOnce()
    expect(getCookie).toHaveBeenCalledWith('access_token')
    expect(mocks.createClient).toHaveBeenCalledWith({
      baseUrl: 'https://api.example.test',
      cache: 'no-store',
      fetch: globalThis.fetch,
      headers: { Cookie: 'access_token=access-token-value' },
    })
  })

  it('fails clearly when the backend API base URL is absent', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', '')

    await expect(createServerApiClient()).rejects.toThrow(
      'BACKEND_API_BASE_URL is required to create the server API client.',
    )
    expect(mocks.cookies).not.toHaveBeenCalled()
    expect(mocks.createClient).not.toHaveBeenCalled()
  })
})
