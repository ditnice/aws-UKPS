import { UserOrganisationContext } from '@/components/Layout/Header/Header'

export const createLinks = (me: UserOrganisationContext | undefined) => {
  if (!me) return []

  return [
    {
      href: `/portal/organisations/${me.organisationId}/records`,
      label: 'View and\nmanage records',
    },
    ...(me.userRole === 'Standard'
      ? []
      : [
          {
            href: `/portal/organisations/${me.organisationId}`,
            label: 'Manage my\norganisation',
          },
        ]),
    {
      href: '/portal/user/me',
      label: 'Manage my\ndetails',
    },
  ]
}
