const organisationActions = ['updated-details'] as const
export type OrganisationAction = (typeof organisationActions)[number]

export type OrganisationActionSearchParams = {
  action?: string
}

type OrganisationActionResult = {
  action: OrganisationAction
}

const isOrganisationAction = (value: string | undefined): value is OrganisationAction => {
  return organisationActions.includes(value as OrganisationAction)
}

export function parseOrganisationAction({
  action,
}: OrganisationActionSearchParams): OrganisationActionResult | undefined {
  return isOrganisationAction(action) ? { action } : undefined
}
