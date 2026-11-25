import { createRemoteJWKSet, jwtVerify } from 'jose'
import { NextRequest, NextResponse } from 'next/server'

import { getServerEnv } from '@/env/server'
import { signInPath } from '@/lib/auth/routing'

let cognitoJwks: ReturnType<typeof createRemoteJWKSet> | undefined

export const config = {
  matcher: ['/:path*'],
}

const portalPathPattern = /^\/portal(\/|$)/

export async function proxy(req: NextRequest) {
  const env = getServerEnv()
  const response = portalPathPattern.test(req.nextUrl.pathname)
    ? await authorisePortalRequest(req, env)
    : NextResponse.next()

  if (env.ROBOTS_NOINDEX) {
    response.headers.set('X-Robots-Tag', 'noindex, nofollow')
  }

  return response
}

async function authorisePortalRequest(req: NextRequest, env: ReturnType<typeof getServerEnv>) {
  const {
    AUTHENTICATION_MODE,
    COGNITO_ISSUER: cognitoIssuer,
    COGNITO_CLIENT_ID: cognitoClientId,
  } = env
  const accessToken = req.cookies.get('access_token')?.value

  if (AUTHENTICATION_MODE === 'DEV') {
    return NextResponse.next()
  }

  if (accessToken) {
    try {
      // Env validation guarantees both outside DEV mode, but its types cannot express that.
      if (!cognitoIssuer) {
        throw Error('Cognito issuer is not configured')
      }

      if (!cognitoClientId) {
        throw Error('Cognito client ID is not configured')
      }

      cognitoJwks ??= createRemoteJWKSet(new URL(`${cognitoIssuer}/.well-known/jwks.json`))
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
