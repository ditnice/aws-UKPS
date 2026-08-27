import { createRemoteJWKSet, jwtVerify } from 'jose'
import { NextRequest, NextResponse } from 'next/server'

import { env } from '@/env/server'
import { signInPath } from '@/lib/auth/routing'

const cognitoIssuer = env.COGNITO_ISSUER
const cognitoClientId = env.COGNITO_CLIENT_ID
const authenticationMode = env.AUTHENTICATION_MODE
const cognitoJwks = cognitoIssuer ? createRemoteJWKSet(getCognitoJwksUrl(cognitoIssuer)) : undefined

function getCognitoJwksUrl(issuer: string): URL {
  const url = new URL(issuer)
  url.pathname = `${url.pathname.replace(/\/$/, '')}/.well-known/jwks.json`
  return url
}

export const config = {
  matcher: ['/portal/:path*'],
}

export async function proxy(req: NextRequest) {
  const accessToken = req.cookies.get('access_token')?.value

  if (authenticationMode === 'DEV') {
    return NextResponse.next()
  }

  if (accessToken) {
    try {
      if (!cognitoIssuer) {
        throw Error('Cognito issuer is not configured')
      }

      if (!cognitoClientId) {
        throw Error('Cognito client ID is not configured')
      }

      if (!cognitoJwks) {
        throw Error('Cognito JWKS is not configured')
      }
      const { payload } = await jwtVerify(accessToken, cognitoJwks, {
        issuer: cognitoIssuer,
      })

      if (payload.token_use === 'access' && payload.client_id === cognitoClientId) {
        const requestHeaders = new Headers(req.headers)
        requestHeaders.set('x-ukps-return-to', `${req.nextUrl.pathname}${req.nextUrl.search}`)

        return NextResponse.next({
          request: {
            headers: requestHeaders,
          },
        })
      }
    } catch (error) {
      console.error('Failed to verify Cognito access token', {
        error: error instanceof Error ? error.message : error,
        path: req.nextUrl.pathname,
      })
    }
  }

  const signInUrl = req.nextUrl.clone()
  signInUrl.pathname = signInPath
  signInUrl.search = ''
  signInUrl.searchParams.set('returnTo', `${req.nextUrl.pathname}${req.nextUrl.search}`)

  return NextResponse.redirect(signInUrl)
}
