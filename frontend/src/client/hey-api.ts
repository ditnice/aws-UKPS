// import 'server-only'

import { postAuthRefresh } from './generated'

import type { CreateClientConfig } from './generated/client.gen'

export const createClientConfig: CreateClientConfig = (config) => ({
  ...config,
  baseUrl: process.env.BACKEND_API_BASE_URL ?? 'https://localhost:7180',
  fetch: async (request, config) => {
    const response = await fetch(request, config)
    const authHeader = response.headers.get('www-authenticate')
    if (response.status === 401 && hasTokenExpired(authHeader)) {
      console.log('Access token has expired. Refreshing token.')
      const cookieValue = getCookie('csrf_token')
      const authRefreshResult = await postAuthRefresh({
        credentials: 'include',
        headers: { 'X-CSRF-Token': cookieValue },
      })
      if (authRefreshResult.error) {
        return response
      } else {
        return await fetch(request, config)
      }
    }
    return response
  },
})

function getCookie(name: string): string | null {
  const cookies = document.cookie.split(';')

  for (const cookie of cookies) {
    const [key, value] = cookie.trim().split('=')

    if (key === name) {
      return decodeURIComponent(value)
    }
  }

  return null
}

function hasTokenExpired(authHeader: string | null) {
  return (
    authHeader &&
    authHeader.includes('Bearer') &&
    authHeader.includes('error="invalid_token"') &&
    authHeader.includes('token expired')
  )
}
