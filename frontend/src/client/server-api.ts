import { cookies, headers } from 'next/headers'
import { redirect, unstable_rethrow } from 'next/navigation'
import 'server-only'

import { buildSignInHref } from '@/lib/auth/routing'

import { AuthenticationFailCode } from './generated'
import { createClient } from './generated/client'

import type { Client } from './generated/client'

const fallbackReturnTo = '/portal'

export async function createServerApiClient(): Promise<Client> {
  const baseUrl = process.env.BACKEND_API_BASE_URL

  if (!baseUrl) {
    throw new Error('BACKEND_API_BASE_URL is required to create the server API client.')
  }

  const accessToken = (await cookies()).get('access_token')?.value
  const returnTo = (await headers()).get('x-ukps-return-to') ?? fallbackReturnTo

  const client: Client = createClient({
    baseUrl,
    cache: 'no-store',
    fetch: createServerFetch(returnTo),
    headers: accessToken ? { Cookie: `access_token=${accessToken}` } : undefined,
  })

  client.interceptors.error.use((err) => {
    unstable_rethrow(err)
    return err
  })

  return client
}

function createServerFetch(returnTo: string): typeof fetch {
  return async (input, init) => {
    const response = await globalThis.fetch(input, init)

    if (response.status === 401 && !isAuthRequest(input)) {
      const content = await response
        .clone()
        .json()
        .catch(() => null)

      if (isAuthorisationFailCode(content?.code)) {
        redirectToAuthenticationError(content?.code)
      }

      redirect(buildSignInHref(returnTo))
    }

    return response
  }
}

function isAuthorisationFailCode(value: unknown): value is AuthenticationFailCode {
  return (
    typeof value === 'string' &&
    Object.values(AuthenticationFailCode).includes(value as AuthenticationFailCode)
  )
}

function isAuthRequest(input: RequestInfo | URL): boolean {
  const requestUrl = input instanceof Request ? input.url : input.toString()

  try {
    return new URL(requestUrl).pathname.startsWith('/auth/')
  } catch {
    return requestUrl === '/auth' || requestUrl.startsWith('/auth/')
  }
}

function redirectToAuthenticationError(code: AuthenticationFailCode) {
  redirect(`/auth/error?code=${code}`)
}
