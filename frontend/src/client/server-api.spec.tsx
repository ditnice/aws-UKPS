import { afterEach, describe, expect, it, vi } from 'vitest'

import { createServerApiClient } from './server-api'

const mocks = vi.hoisted(() => ({
  cookies: vi.fn(),
  createClient: vi.fn(),
  headers: vi.fn(),
  redirect: vi.fn(),
}))

vi.mock('next/headers', () => ({ cookies: mocks.cookies, headers: mocks.headers }))
vi.mock('next/navigation', () => ({ redirect: mocks.redirect }))
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
    mocks.headers.mockResolvedValue({ get: vi.fn(() => '/portal/organisations/1?page=2') })
    const client = { request: vi.fn() }
    mocks.createClient.mockReturnValue(client)

    await expect(createServerApiClient()).resolves.toBe(client)
    expect(getCookie).toHaveBeenCalledOnce()
    expect(getCookie).toHaveBeenCalledWith('access_token')
    expect(mocks.createClient).toHaveBeenCalledWith({
      baseUrl: 'https://api.example.test',
      cache: 'no-store',
      fetch: expect.any(Function),
      headers: { Cookie: 'access_token=access-token-value' },
    })
  })

  it('redirects server API requests to sign in when the backend returns 401', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', 'https://api.example.test')
    mocks.cookies.mockResolvedValue({ get: vi.fn(() => undefined) })
    mocks.headers.mockResolvedValue({ get: vi.fn(() => '/portal/organisations/1?page=2') })
    mocks.createClient.mockImplementation((config) => config)
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status: 401 }))
    vi.stubGlobal('fetch', fetchMock)

    const client = await createServerApiClient()
    await client.fetch('https://api.example.test/organisations/1')

    expect(mocks.redirect).toHaveBeenCalledWith(
      '/auth/sign-in?returnTo=%2Fportal%2Forganisations%2F1%3Fpage%3D2',
    )
  })

  it('uses a portal returnTo fallback when no forwarded route header is available', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', 'https://api.example.test')
    mocks.cookies.mockResolvedValue({ get: vi.fn(() => undefined) })
    mocks.headers.mockResolvedValue({ get: vi.fn(() => null) })
    mocks.createClient.mockImplementation((config) => config)
    vi.stubGlobal(
      'fetch',
      vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status: 401 })),
    )

    const client = await createServerApiClient()
    await client.fetch('https://api.example.test/users')

    expect(mocks.redirect).toHaveBeenCalledWith('/auth/sign-in?returnTo=%2Fportal')
  })

  it('does not redirect server auth requests that return 401', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', 'https://api.example.test')
    mocks.cookies.mockResolvedValue({ get: vi.fn(() => undefined) })
    mocks.headers.mockResolvedValue({ get: vi.fn(() => '/portal') })
    mocks.createClient.mockImplementation((config) => config)
    vi.stubGlobal(
      'fetch',
      vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status: 401 })),
    )

    const client = await createServerApiClient()
    const response = await client.fetch('https://api.example.test/auth/refresh')

    expect(response.status).toBe(401)
    expect(mocks.redirect).not.toHaveBeenCalled()
  })

  it('passes through successful server API responses', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', 'https://api.example.test')
    mocks.cookies.mockResolvedValue({ get: vi.fn(() => undefined) })
    mocks.headers.mockResolvedValue({ get: vi.fn(() => '/portal') })
    mocks.createClient.mockImplementation((config) => config)
    const successResponse = new Response('{}', { status: 200 })
    vi.stubGlobal('fetch', vi.fn<typeof fetch>().mockResolvedValue(successResponse))

    const client = await createServerApiClient()

    await expect(client.fetch('https://api.example.test/users')).resolves.toBe(successResponse)
    expect(mocks.redirect).not.toHaveBeenCalled()
  })

  it('fails clearly when the backend API base URL is absent', async () => {
    vi.stubEnv('BACKEND_API_BASE_URL', '')

    await expect(createServerApiClient()).rejects.toThrow(
      'BACKEND_API_BASE_URL is required to create the server API client.',
    )
    expect(mocks.cookies).not.toHaveBeenCalled()
    expect(mocks.headers).not.toHaveBeenCalled()
    expect(mocks.createClient).not.toHaveBeenCalled()
  })
})
