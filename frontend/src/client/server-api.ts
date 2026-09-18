import { isRedirectError } from 'next/dist/client/components/redirect-error'
import { cookies, headers } from 'next/headers'
import { redirect } from 'next/navigation'
import 'server-only'

import { buildSignInHref } from '@/lib/auth/routing'

import { AuthenticationFailCode } from './generated'
import { createClient } from './generated/client'

import type { Client, RequestOptions, RequestResult } from './generated/client'

const fallbackReturnTo = '/portal'

export async function createServerApiClient(): Promise<Client> {
  const baseUrl = process.env.BACKEND_API_BASE_URL

  if (!baseUrl) {
    throw new Error('BACKEND_API_BASE_URL is required to create the server API client.')
  }

  const accessToken = (await cookies()).get('access_token')?.value
  const returnTo = (await headers()).get('x-ukps-return-to') ?? fallbackReturnTo

  const redirectHandlingWrapper = <
    TData = unknown,
    TError = unknown,
    ThrowOnError extends boolean = false,
  >(
    methodFn: <TData = unknown, TError = unknown, ThrowOnError extends boolean = false>(
      options: Omit<RequestOptions<TData, ThrowOnError>, 'method'>,
    ) => RequestResult<TData, TError, ThrowOnError>,
  ) => {
    return (options: Omit<RequestOptions<TData, ThrowOnError>, 'method'>) => {
      return methodFn<TData, TError, ThrowOnError>(options).then((response) => {
        if ('error' in response && isRedirectError(response.error)) {
          throw response.error
        }
        return response
      }) as RequestResult<TData, TError, ThrowOnError>
    }
  }

  const client: Client = createClient({
    baseUrl,
    cache: 'no-store',
    fetch: createServerFetch(returnTo),
    headers: accessToken ? { Cookie: `access_token=${accessToken}` } : undefined,
  })

  /// This wrapper is added because HeyAPI catches all errors throws as default.
  // This is what we want, except for the error throw when calling the redirect function.
  return {
    ...client,
    get: redirectHandlingWrapper(client.get),
    patch: redirectHandlingWrapper(client.patch),
    post: redirectHandlingWrapper(client.post),
    put: redirectHandlingWrapper(client.put),
    delete: redirectHandlingWrapper(client.delete),
    connect: redirectHandlingWrapper(client.connect),
    options: redirectHandlingWrapper(client.options),
    trace: redirectHandlingWrapper(client.trace),
  }
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
