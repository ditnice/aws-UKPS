import { AuthenticationFailCode } from '@/client/generated/types.gen'

export const signInPath = '/auth/sign-in'
export const selectOrganisationPath = '/portal/organisations/select'
export const switchOrganisationPath = '/portal/organisations/switch'
export const routeOnSuccessfulAuth = '/portal'

export function buildSignInHref(returnTo: string | undefined): string {
  return buildHref(signInPath, { returnTo: getSafeReturnTo(returnTo) })
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

export function getSafeReturnTo(returnTo: string | undefined): string | undefined {
  const trimmedReturnTo = returnTo?.trim()

  if (!trimmedReturnTo || !trimmedReturnTo.startsWith('/') || trimmedReturnTo.startsWith('//')) {
    return undefined
  }

  return trimmedReturnTo
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
