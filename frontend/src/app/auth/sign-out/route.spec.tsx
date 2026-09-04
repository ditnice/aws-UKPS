import { afterEach, describe, expect, it, vi } from 'vitest'

import { POST } from './route'

const mocks = vi.hoisted(() => ({
  cookies: vi.fn(),
}))

vi.mock('next/headers', () => ({ cookies: mocks.cookies }))

afterEach(() => {
  vi.clearAllMocks()
  vi.unstubAllGlobals()
})

describe('POST /auth/sign-out', () => {
  it('calls backend sign-out, clears the auth cookies, and redirects home', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 200 }))
    const set = vi.fn()
    const get = vi.fn((name: string) =>
      name === 'csrf_token' ? { value: 'csrf-token' } : undefined,
    )
    vi.stubGlobal('fetch', fetchMock)
    mocks.cookies.mockResolvedValue({ get, set })

    const response = await POST(
      new Request('https://example.test/auth/sign-out', {
        headers: { cookie: 'csrf_token=csrf-token; refresh_token=refresh-token' },
      }),
    )

    expect(response.status).toBe(303)
    expect(response.headers.get('location')).toBe('https://example.test/')
    expect(fetchMock).toHaveBeenCalledWith(
      new URL('https://example.test/backend-api/auth/sign-out'),
      {
        method: 'POST',
        cache: 'no-store',
        headers: {
          Cookie: 'csrf_token=csrf-token; refresh_token=refresh-token',
          Origin: 'https://example.test',
          'X-CSRF-Token': 'csrf-token',
        },
        redirect: 'manual',
      },
    )

    expect(set).toHaveBeenCalledWith('access_token', '', {
      path: '/',
      httpOnly: true,
      secure: true,
      sameSite: 'lax',
      maxAge: 0,
    })
    expect(set).toHaveBeenCalledWith('csrf_token', '', {
      path: '/',
      secure: true,
      sameSite: 'strict',
      maxAge: 0,
    })
    expect(set).toHaveBeenCalledWith('refresh_token', '', {
      path: '/backend-api/auth',
      httpOnly: true,
      secure: true,
      maxAge: 0,
    })
    expect(set).toHaveBeenCalledWith('refresh_token', '', {
      path: '/backend-api/auth/refresh',
      httpOnly: true,
      secure: true,
      maxAge: 0,
    })
  })
})
