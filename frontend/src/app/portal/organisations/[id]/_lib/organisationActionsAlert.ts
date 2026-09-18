const organisationActions = ['updated-details'] as const
export type OrganisationAction = (typeof organisationActions)[number]

export type OrganisationActionSearchParams = {
  action?: string
}

export type UserActionResult = {
  action: OrganisationAction
}

const isOrganisationAction = (value: string | undefined): value is OrganisationAction => {
  return organisationActions.includes(value as OrganisationAction)
}

export function parseOrganisationAction({
  action,
}: OrganisationActionSearchParams): UserActionResult | undefined {
  return isOrganisationAction(action) ? { action } : undefined
}
