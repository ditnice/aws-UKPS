import { NextRequest } from 'next/server'
import { afterEach, describe, expect, it, vi } from 'vitest'

const issuer = 'https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test'
const clientId = 'test-client-id'
const createRemoteJWKSet = vi.fn(() => 'jwks')
const jwtVerify = vi.fn()

function createRequest(url: string, cookie?: string) {
  return new NextRequest(url, cookie ? { headers: { cookie } } : undefined)
}

async function loadProxy(
  overrides: {
    authenticationMode?: string
    issuer?: string
    clientId?: string
    robotsNoIndex?: string
  } = {},
) {
  vi.resetModules()
  vi.doMock('jose', () => ({ createRemoteJWKSet, jwtVerify }))
  const config = {
    issuer,
    clientId,
    authenticationMode: undefined,
    robotsNoIndex: undefined,
    ...overrides,
  }
  vi.stubEnv('COGNITO_ISSUER', config.issuer)
  vi.stubEnv('COGNITO_CLIENT_ID', config.clientId)
  vi.stubEnv('AUTHENTICATION_MODE', config.authenticationMode)
  vi.stubEnv('ROBOTS_NOINDEX', config.robotsNoIndex)

  return import('./proxy')
}

afterEach(() => {
  vi.unstubAllEnvs()
  vi.doUnmock('jose')
  createRemoteJWKSet.mockClear()
  jwtVerify.mockReset()
  vi.restoreAllMocks()
})

describe('proxy', () => {
  it('matches all routes', async () => {
    const { config } = await loadProxy()

    expect(config.matcher).toEqual(['/:path*'])
  })

  it('allows non-portal requests without a token', async () => {
    const { proxy } = await loadProxy()
    const response = await proxy(createRequest('https://frontend.example/portal-help'))

    expect(response.status).toBe(200)
    expect(response.headers.get('location')).toBeNull()
    expect(jwtVerify).not.toHaveBeenCalled()
  })

  it('does not send X-Robots-Tag when ROBOTS_NOINDEX is not set', async () => {
    const { proxy } = await loadProxy()
    const response = await proxy(createRequest('https://frontend.example/'))

    expect(response.headers.get('x-robots-tag')).toBeNull()
  })

  it('does not send X-Robots-Tag when ROBOTS_NOINDEX is not true', async () => {
    const { proxy } = await loadProxy({ robotsNoIndex: 'false' })
    const response = await proxy(createRequest('https://frontend.example/'))

    expect(response.headers.get('x-robots-tag')).toBeNull()
  })

  it('sends X-Robots-Tag on public pages when ROBOTS_NOINDEX is true', async () => {
    const { proxy } = await loadProxy({ robotsNoIndex: 'true' })
    const response = await proxy(createRequest('https://frontend.example/'))

    expect(response.status).toBe(200)
    expect(response.headers.get('x-robots-tag')).toBe('noindex, nofollow')
  })

  it('sends X-Robots-Tag on sign-in redirects when ROBOTS_NOINDEX is true', async () => {
    const { proxy } = await loadProxy({ robotsNoIndex: 'true' })
    const response = await proxy(createRequest('https://frontend.example/portal'))

    expect(response.status).toBe(307)
    expect(response.headers.get('x-robots-tag')).toBe('noindex, nofollow')
  })

  it('sends X-Robots-Tag on authorised portal requests when ROBOTS_NOINDEX is true', async () => {
    jwtVerify.mockResolvedValue({ payload: { client_id: clientId, token_use: 'access' } })
    const { proxy } = await loadProxy({ robotsNoIndex: 'true' })
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=valid-token'),
    )

    expect(response.status).toBe(200)
    expect(response.headers.get('x-robots-tag')).toBe('noindex, nofollow')
  })

  it('allows portal requests with a valid access token', async () => {
    jwtVerify.mockResolvedValue({ payload: { client_id: clientId, token_use: 'access' } })
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=valid-token'),
    )

    expect(response.status).toBe(200)
    expect(response.headers.get('location')).toBeNull()
    expect(response.headers.get('x-middleware-request-x-ukps-return-to')).toBe('/portal')
    expect(jwtVerify).toHaveBeenCalledWith('valid-token', 'jwks', { issuer })
  })

  it('allows portal requests without a token in development mode', async () => {
    const { proxy } = await loadProxy({ authenticationMode: 'DEV' })
    const response = await proxy(
      createRequest('https://frontend.example/portal/organisations/1?page=2'),
    )

    expect(response.status).toBe(200)
    expect(response.headers.get('location')).toBeNull()
    expect(jwtVerify).not.toHaveBeenCalled()
  })

  it('forwards the portal path and query string for server-side returnTo redirects', async () => {
    jwtVerify.mockResolvedValue({ payload: { client_id: clientId, token_use: 'access' } })
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest(
        'https://frontend.example/portal/organisations/1?page=2',
        'access_token=valid-token',
      ),
    )

    expect(response.headers.get('x-middleware-request-x-ukps-return-to')).toBe(
      '/portal/organisations/1?page=2',
    )
  })

  it('redirects portal requests without an access token to sign in', async () => {
    const { proxy } = await loadProxy()
    const response = await proxy(createRequest('https://frontend.example/portal'))

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
  })

  it('logs a missing issuer and redirects requests with an access token', async () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})
    const { proxy } = await loadProxy({ issuer: undefined })
    const response = await proxy(
      createRequest(
        'https://frontend.example/portal/organisations/1?page=2',
        'access_token=test-token',
      ),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal%2Forganisations%2F1%3Fpage%3D2',
    )
    expect(jwtVerify).not.toHaveBeenCalled()
    expect(consoleError).toHaveBeenCalledWith('Failed to verify Cognito access token', {
      error: 'Cognito issuer is not configured',
      path: '/portal/organisations/1',
    })
  })

  it('logs a missing client ID and redirects requests with an access token', async () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})
    const { proxy } = await loadProxy({ clientId: undefined })
    const response = await proxy(
      createRequest(
        'https://frontend.example/portal/organisations/1?page=2',
        'access_token=test-token',
      ),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal%2Forganisations%2F1%3Fpage%3D2',
    )
    expect(jwtVerify).not.toHaveBeenCalled()
    expect(consoleError).toHaveBeenCalledWith('Failed to verify Cognito access token', {
      error: 'Cognito client ID is not configured',
      path: '/portal/organisations/1',
    })
  })

  it('preserves the portal path and query string in returnTo', async () => {
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal/organisations/1?tab=users'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal%2Forganisations%2F1%3Ftab%3Dusers',
    )
  })

  it('redirects portal requests with a malformed access token', async () => {
    jwtVerify.mockRejectedValue(new Error('Invalid compact JWS'))
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=not-a-jwt'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
  })

  it('logs string verification failures and redirects to sign in', async () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})
    jwtVerify.mockRejectedValue('verification failed')
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=test-token'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
    expect(consoleError).toHaveBeenCalledWith('Failed to verify Cognito access token', {
      error: 'verification failed',
      path: '/portal',
    })
  })

  it('redirects portal requests with an expired access token', async () => {
    jwtVerify.mockRejectedValue(new Error('JWT expired'))
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=expired-token'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
  })

  it('redirects portal requests with the wrong issuer', async () => {
    jwtVerify.mockRejectedValue(new Error('unexpected iss claim value'))
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=wrong-issuer-token'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
  })

  it('redirects portal requests with the wrong client ID', async () => {
    jwtVerify.mockResolvedValue({ payload: { client_id: 'wrong-client-id', token_use: 'access' } })
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=wrong-client-token'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
  })

  it('redirects portal requests with the wrong token use', async () => {
    jwtVerify.mockResolvedValue({ payload: { client_id: clientId, token_use: 'id' } })
    const { proxy } = await loadProxy()
    const response = await proxy(
      createRequest('https://frontend.example/portal', 'access_token=id-token'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
  })
})
