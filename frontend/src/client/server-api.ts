import { cookies } from 'next/headers'
import 'server-only'

import { createClient } from './generated/client'

export async function createServerApiClient() {
  const baseUrl = process.env.BACKEND_API_BASE_URL

  if (!baseUrl) {
    throw new Error('BACKEND_API_BASE_URL is required to create the server API client.')
  }

  const accessToken = (await cookies()).get('access_token')?.value

  return createClient({
    baseUrl,
    cache: 'no-store',
    fetch: globalThis.fetch,
    headers: accessToken ? { Cookie: `access_token=${accessToken}` } : undefined,
  })
}
