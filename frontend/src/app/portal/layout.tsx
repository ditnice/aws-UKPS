import { getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { ApplicationLayout } from '@/components/Layout/ApplicationLayout'
import { inter, lora } from '@/styles/fonts'
import '@/styles/global.scss'

import type { ReactNode } from 'react'

export const metadata = {
  description: 'An essential first step to market access for medicines.',
  title: 'UK PharmaScan Portal',
}

export default async function PortalLayout({ children }: { children: ReactNode }) {
  const { data: me } = await getUsersMe({ client: await createServerApiClient() })

  const links = [
    {
      href: `/portal/organisations/${me?.organisationId}/records`,
      label: (
        <>
          View and
          <br />
          manage records
        </>
      ),
    },
    ...(me?.userRole === 'Standard'
      ? []
      : [
          {
            href: `/portal/organisations/${me?.organisationId}`,
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
  return (
    <html className={`${inter.variable} ${lora.variable}`} lang="en">
      <body>
        <ApplicationLayout links={links}>{children}</ApplicationLayout>
      </body>
    </html>
  )
}
