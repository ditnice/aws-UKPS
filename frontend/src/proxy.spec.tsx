import { NextRequest } from 'next/server'
import { describe, expect, it } from 'vitest'

import { config, proxy } from './proxy'

function createRequest(url: string, cookie?: string) {
  return new NextRequest(url, cookie ? { headers: { cookie } } : undefined)
}

describe('proxy', () => {
  it('matches portal routes', () => {
    expect(config.matcher).toEqual(['/portal/:path*'])
  })

  it('allows portal requests with an access token', () => {
    const response = proxy(createRequest('https://frontend.example/portal', 'access_token=abc'))

    expect(response.status).toBe(200)
    expect(response.headers.get('location')).toBeNull()
  })

  it('redirects portal requests without an access token to sign in', () => {
    const response = proxy(createRequest('https://frontend.example/portal'))

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal',
    )
  })

  it('preserves the portal path and query string in returnTo', () => {
    const response = proxy(
      createRequest('https://frontend.example/portal/organisations/1?tab=users'),
    )

    expect(response.status).toBe(307)
    expect(response.headers.get('location')).toBe(
      'https://frontend.example/auth/sign-in?returnTo=%2Fportal%2Forganisations%2F1%3Ftab%3Dusers',
    )
  })
})
