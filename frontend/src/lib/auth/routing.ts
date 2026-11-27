import { AuthenticationFailCode } from '@/client/generated/types.gen'

export const signInPath = '/auth/sign-in'
export const selectOrganisationPath = '/portal/organisations/select'
export const switchOrganisationPath = '/portal/organisations/switch'

// A full page load rather than router.push, so the portal is rendered from scratch with the new
// session cookies and nothing cached while signed out (such as the layout or header) is reused.
// replace rather than assign so Back does not return to the sign-in or MFA forms, whose
// authentication session has already been used.
export function navigateOnSuccessfulAuth(returnTo?: string): void {
  globalThis.location.replace(buildSelectOrganisationHref(returnTo))
}

export type SignInOptions = {
  emailVerified?: true
}
export function buildSignInHref(
  returnTo: string | undefined,
  signInOptions?: SignInOptions,
): string {
  const additionalSearchParams: Record<string, string | undefined> = {}

  if (signInOptions?.emailVerified !== undefined) {
    additionalSearchParams.emailVerified = String(signInOptions.emailVerified)
  }

  return buildHref(signInPath, { returnTo: getSafeReturnTo(returnTo), ...additionalSearchParams })
}

// Links to the organisation picker are not nested, so the picker never returns to itself.
export function buildSelectOrganisationHref(returnTo: string | undefined): string {
  const safeReturnTo = getSafeReturnTo(returnTo)

  if (safeReturnTo && isPath(safeReturnTo, selectOrganisationPath)) {
    return safeReturnTo
  }

  return buildHref(selectOrganisationPath, { returnTo: safeReturnTo })
}

// Where to send the user when an API problem response says they must select an organisation, or
// confirm switching to the one they requested, before continuing to returnTo.
export function getOrganisationSelectionHref(
  problem: { code?: unknown; organisationId?: unknown } | null,
  returnTo: string | undefined,
): string | undefined {
  switch (problem?.code) {
    case AuthenticationFailCode.SELECTED_ORGANISATION_MISMATCH:
      return typeof problem.organisationId === 'number'
        ? buildHref(switchOrganisationPath, {
            organisationId: `${problem.organisationId}`,
            returnTo: getSafeReturnTo(returnTo),
          })
        : undefined
    case AuthenticationFailCode.SELECTED_ORGANISATION_REQUIRED:
    case AuthenticationFailCode.SELECTED_ORGANISATION_IS_NOT_VALID:
      return buildSelectOrganisationHref(returnTo)
    default:
      return undefined
  }
}

// Where to go once an organisation is selected. A returnTo for another organisation would only
// send the user back to the picker, so it falls back to the selected organisation's records.
export function getOrganisationLandingHref(
  organisationId: number,
  returnTo: string | undefined,
): string {
  const safeReturnTo = getSafeReturnTo(returnTo)
  const returnToOrganisationId = safeReturnTo?.match(
    /^\/portal\/organisations\/(\d+)(?:[/?#]|$)/,
  )?.[1]

  if (
    !safeReturnTo ||
    isPath(safeReturnTo, selectOrganisationPath) ||
    isPath(safeReturnTo, switchOrganisationPath) ||
    (returnToOrganisationId !== undefined && returnToOrganisationId !== `${organisationId}`)
  ) {
    return `/portal/organisations/${organisationId}/records`
  }

  return safeReturnTo
}

// The base getSafeReturnTo parses against. It is only compared with the parsed origin and is never
// navigated to or returned: the result is the path, query and fragment alone.
// - Fixed rather than location.origin, so the check behaves the same on the server, where there is
//   no location.
// - The scheme must be http(s). Browsers only treat "\" as "/" and "//x" as a host for these
//   "special" schemes, so with a base such as "x:/", "/\evil.com" would parse as a harmless path
//   here but still reach the browser as "//evil.com".
// - .invalid is reserved (RFC 6761) and never resolves, so the base cannot name a real site.
const returnToBase = 'http://return-to.invalid'

// Browsers treat a backslash as a slash and strip tabs and newlines inside URLs, so a prefix check
// alone would let "/\evil.com" or "/<tab>/evil.com" through as "//evil.com". Parse the value the way
// a browser would, then check both the origin and the normalised path, since dot segments can also
// produce a leading "//" (for example "/.//evil.com").
export function getSafeReturnTo(returnTo: string | undefined): string | undefined {
  const trimmedReturnTo = returnTo?.trim()

  if (!trimmedReturnTo || !trimmedReturnTo.startsWith('/')) {
    return undefined
  }

  let url: URL
  try {
    url = new URL(trimmedReturnTo, returnToBase)
  } catch {
    return undefined
  }

  // With an http(s) base the pathname always starts with "/" and any "\" in it has already become
  // "/", so a leading "//" is the only unsafe shape left to check for.
  const safeReturnTo = `${url.pathname}${url.search}${url.hash}`
  if (url.origin !== returnToBase || safeReturnTo.startsWith('//')) {
    return undefined
  }

  return safeReturnTo
}

function buildHref(path: string, params: Record<string, string | undefined>): string {
  const query = new URLSearchParams(
    Object.entries(params).filter((entry): entry is [string, string] => entry[1] !== undefined),
  ).toString()

  return query ? `${path}?${query}` : path
}

function isPath(path: string, expected: string): boolean {
  return path === expected || ['/', '?', '#'].some((x) => path.startsWith(`${expected}${x}`))
}
