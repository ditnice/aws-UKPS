import { createRemoteJWKSet, jwtVerify } from 'jose'
import { NextRequest, NextResponse } from 'next/server'

// Note: this file runs on the Edge runtime (Next's default for middleware/proxy.ts;
// no `experimental.nodeMiddleware` is configured). Pino is Node-only and cannot be
// imported here — see frontend/src/lib/logger.ts for the Node-runtime logging convention.

const signInPath = '/auth/sign-in'
const cognitoIssuer = process.env.COGNITO_ISSUER
const cognitoClientId = process.env.COGNITO_CLIENT_ID
const cognitoJwks = cognitoIssuer
  ? createRemoteJWKSet(new URL(`${cognitoIssuer}/.well-known/jwks.json`))
  : undefined

export const config = {
  matcher: ['/portal/:path*'],
}

export async function proxy(req: NextRequest) {
  const accessToken = req.cookies.get('access_token')?.value

  if (accessToken && cognitoIssuer && cognitoClientId && cognitoJwks) {
    try {
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
    } catch {
      // Invalid, expired, or untrusted tokens should use the standard sign-in redirect.
    }
  }

  const signInUrl = req.nextUrl.clone()
  signInUrl.pathname = signInPath
  signInUrl.search = ''
  signInUrl.searchParams.set('returnTo', `${req.nextUrl.pathname}${req.nextUrl.search}`)

  return NextResponse.redirect(signInUrl)
}
