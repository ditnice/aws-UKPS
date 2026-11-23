export const csrfCookieName = 'csrf_token'
export const csrfHeaderName = 'X-CSRF-Token'
/* With AUTHENTICATION_MODE=DEV, the backend's dev authentication acts as this cookie's role
 * (Super, Champion or Standard), so E2E tests can switch user. Only forwarded in DEV mode. */
export const devRoleCookieName = 'ukps_dev_role'

export function isDevAuthenticationMode(): boolean {
  return process.env.AUTHENTICATION_MODE === 'DEV'
}

export function getCookie(name: string): string | null {
  if (typeof document === 'undefined') {
    return null
  }

  for (const cookie of document.cookie.split(';')) {
    const separator = cookie.indexOf('=')
    const key = (separator === -1 ? cookie : cookie.slice(0, separator)).trim()

    if (key === name) {
      return decodeURIComponent(separator === -1 ? '' : cookie.slice(separator + 1))
    }
  }

  return null
}
