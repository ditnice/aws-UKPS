import { NextRequest, NextResponse } from 'next/server'

export const config = {
  matcher: ['/portal/:path*'],
}

export function middleware(_req: NextRequest) {
  return NextResponse.next()
}
