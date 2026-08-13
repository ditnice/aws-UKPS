export const routeOnSuccessfulAuth = '/portal'

export function getSafeReturnTo(returnTo: string | undefined): string | undefined {
  const trimmedReturnTo = returnTo?.trim()

  if (!trimmedReturnTo || !trimmedReturnTo.startsWith('/') || trimmedReturnTo.startsWith('//')) {
    return undefined
  }

  return trimmedReturnTo
}
