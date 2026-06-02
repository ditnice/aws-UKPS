import { NextRequest, NextResponse } from 'next/server'

export const config = {
  matcher: ['/portal/:path*'],
}

export function middleware(req: NextRequest) {
  // TODO: verify Cognito JWT (Step 9)
  return NextResponse.next()
}
