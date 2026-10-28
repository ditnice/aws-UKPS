import { ApplicationLayout } from '@/components/Layout/ApplicationLayout'
import { OrganisationSwitcher } from '@/components/Layout/Header/OrganisationSwitcher/OrganisationSwitcher'
import { inter, lora } from '@/styles/fonts'
import '@/styles/global.scss'

import { getUserOrganisationContext } from './_lib/userOrganisationContext'

import type { ReactNode } from 'react'

export const metadata = {
  description: 'An essential first step to market access for medicines.',
  title: 'UK PharmaScan Portal',
}

export default async function PortalLayout({ children }: { children: ReactNode }) {
  const me = await getUserOrganisationContext()
  const links = createLinks(me)
  const headerExtension = me?.hasMultipleOrganisations ? (
    <OrganisationSwitcher organisationName={me.organisationName} />
  ) : (
    <></>
  )

  return (
    <html className={`${inter.variable} ${lora.variable}`} lang="en">
      <body>
        <ApplicationLayout links={links} headerExtension={headerExtension}>
          {children}
        </ApplicationLayout>
      </body>
    </html>
  )
}
