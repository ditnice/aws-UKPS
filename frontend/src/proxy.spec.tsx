import { NextRequest } from 'next/server'
import { afterEach, describe, expect, it, vi } from 'vitest'

const issuer = 'https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_test'
const clientId = 'test-client-id'
const createRemoteJWKSet = vi.fn(() => 'jwks')
const jwtVerify = vi.fn()

function createRequest(url: string, cookie?: string) {
  return new NextRequest(url, cookie ? { headers: { cookie } } : undefined)
}

async function loadProxy() {
  vi.resetModules()
  vi.doMock('jose', () => ({ createRemoteJWKSet, jwtVerify }))
  vi.stubEnv('COGNITO_ISSUER', issuer)
  vi.stubEnv('COGNITO_CLIENT_ID', clientId)

  return import('./proxy')
}

afterEach(() => {
  vi.doUnmock('jose')
  createRemoteJWKSet.mockClear()
  jwtVerify.mockReset()
  vi.unstubAllEnvs()
})

describe('proxy', () => {
  it('matches portal routes', async () => {
    const { config } = await loadProxy()

    expect(config.matcher).toEqual(['/portal/:path*'])
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
