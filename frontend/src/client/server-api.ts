import { cookies, headers } from 'next/headers'
import { redirect } from 'next/navigation'
import 'server-only'

import { env } from '@/env/server'
import { buildSignInHref } from '@/lib/auth/routing'

import { createClient } from './generated/client'

const fallbackReturnTo = '/portal'

export async function createServerApiClient() {
  const accessToken = (await cookies()).get('access_token')?.value
  const returnTo = (await headers()).get('x-ukps-return-to') ?? fallbackReturnTo

  return createClient({
    baseUrl: env.BACKEND_API_BASE_URL,
    cache: 'no-store',
    fetch: createServerFetch(returnTo),
    headers: accessToken ? { Cookie: `access_token=${accessToken}` } : undefined,
  })
}

function createServerFetch(returnTo: string): typeof fetch {
  return async (input, init) => {
    const response = await globalThis.fetch(input, init)

    if (response.status === 401 && !isAuthRequest(input)) {
      redirect(buildSignInHref(returnTo))
    }

    return response
  }
}

function isAuthRequest(input: RequestInfo | URL): boolean {
  const requestUrl = input instanceof Request ? input.url : input.toString()

  try {
    return new URL(requestUrl).pathname.startsWith('/auth/')
  } catch {
    return requestUrl === '/auth' || requestUrl.startsWith('/auth/')
  }
}
