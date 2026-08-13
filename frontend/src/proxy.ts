import { NextRequest, NextResponse } from 'next/server'

const signInPath = '/auth/sign-in'

export const config = {
  matcher: ['/portal/:path*'],
}

export function proxy(req: NextRequest) {
  if (req.cookies.get('access_token')?.value) {
    return NextResponse.next()
  }

  const signInUrl = req.nextUrl.clone()
  signInUrl.pathname = signInPath
  signInUrl.search = ''
  signInUrl.searchParams.set('returnTo', `${req.nextUrl.pathname}${req.nextUrl.search}`)

  return NextResponse.redirect(signInUrl)
}
