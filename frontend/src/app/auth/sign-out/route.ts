import { cookies } from 'next/headers'
import { NextResponse } from 'next/server'

export const runtime = 'nodejs'

export async function POST(request: Request) {
  const cookieStore = await cookies()
  const origin = new URL(request.url).origin
  const csrfToken = cookieStore.get('csrf_token')?.value

  if (csrfToken) {
    try {
      await fetch(new URL('/backend-api/auth/sign-out', origin), {
        method: 'POST',
        cache: 'no-store',
        headers: {
          Cookie: request.headers.get('cookie') ?? '',
          Origin: origin,
          'X-CSRF-Token': csrfToken,
        },
        redirect: 'manual',
      })
    } catch {
      // Local cookie clearing below still signs this browser out if revocation is unavailable.
    }
  }

  cookieStore.set('access_token', '', {
    path: '/',
    httpOnly: true,
    secure: true,
    sameSite: 'lax',
    maxAge: 0,
  })
  cookieStore.set('csrf_token', '', {
    path: '/',
    secure: true,
    sameSite: 'strict',
    maxAge: 0,
  })
  cookieStore.set('refresh_token', '', {
    path: '/backend-api/auth',
    httpOnly: true,
    secure: true,
    maxAge: 0,
  })
  cookieStore.set('refresh_token', '', {
    path: '/backend-api/auth/refresh',
    httpOnly: true,
    secure: true,
    maxAge: 0,
  })

  return NextResponse.redirect(new URL('/', request.url), { status: 303 })
}
