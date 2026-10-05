export const csrfCookieName = 'csrf_token'
export const csrfHeaderName = 'X-CSRF-Token'

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
