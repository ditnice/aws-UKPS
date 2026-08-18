export const signInPath = '/auth/sign-in'
export const routeOnSuccessfulAuth = '/portal'

export function buildSignInHref(returnTo: string | undefined): string {
  const safeReturnTo = getSafeReturnTo(returnTo)

  if (!safeReturnTo) {
    return signInPath
  }

  return `${signInPath}?${new URLSearchParams({ returnTo: safeReturnTo }).toString()}`
}

export function getSafeReturnTo(returnTo: string | undefined): string | undefined {
  const trimmedReturnTo = returnTo?.trim()

  if (!trimmedReturnTo || !trimmedReturnTo.startsWith('/') || trimmedReturnTo.startsWith('//')) {
    return undefined
  }

  return trimmedReturnTo
}
