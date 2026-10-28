import { ApplicationLayout } from '@/components/Layout/ApplicationLayout'
import { inter, lora } from '@/styles/fonts'
import '@/styles/global.scss'

import { getUserOrganisationContext } from './_lib/userOrganisationContext'

import type { ReactNode } from 'react'

export const metadata = {
  description: 'An essential first step to market access for medicines.',
  title: 'UK PharmaScan Portal',
}

export default async function PortalLayout({ children }: { children: ReactNode }) {
  const organisationContext = await getUserOrganisationContext()

  return (
    <html className={`${inter.variable} ${lora.variable}`} lang="en">
      <body>
        <ApplicationLayout organisationContext={organisationContext}>{children}</ApplicationLayout>
      </body>
    </html>
  )
}
