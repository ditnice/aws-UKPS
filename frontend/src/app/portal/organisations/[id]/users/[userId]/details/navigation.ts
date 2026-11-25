type buildViewUserDetailsHrefArgs = {
  organisationId: number
  userId: number
}
export const buildViewUserDetailsHref = ({ userId, organisationId }: buildViewUserDetailsHrefArgs) => {
  return `/portal/organisations/${userId}/users/${organisationId}/details`
}
