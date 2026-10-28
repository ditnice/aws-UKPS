import { UserOrganisationContext } from '@/components/Layout/Header/Header'

export const createLinks = (me: UserOrganisationContext | undefined) => {
  if (!me) return []

  return [
    {
      href: `/portal/organisations/${me.organisationId}/records`,
      label: (
        <>
          View and
          <br />
          manage records
        </>
      ),
    },
    ...(me.userRole === 'Standard'
      ? []
      : [
          {
            href: `/portal/organisations/${me.organisationId}`,
            label: (
              <>
                Manage my
                <br />
                organisation
              </>
            ),
          },
        ]),
    {
      href: '/portal/user/me',
      label: (
        <>
          Manage my
          <br />
          details{' '}
        </>
      ),
    },
  ]
}
