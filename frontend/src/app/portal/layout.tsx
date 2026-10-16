import { ApplicationLayout } from '@/components/Layout/ApplicationLayout'
import { inter, lora } from '@/styles/fonts'
import '@/styles/global.scss'

import { getCurrentOrganisation } from './_lib/currentOrganisation'

import type { ReactNode } from 'react'

export const metadata = {
  description: 'An essential first step to market access for medicines.',
  title: 'UK PharmaScan Portal',
}

export default async function PortalLayout({ children }: { children: ReactNode }) {
  const currentOrganisation = await getCurrentOrganisation()

  return (
    <html className={`${inter.variable} ${lora.variable}`} lang="en">
      <body>
        <ApplicationLayout currentOrganisation={currentOrganisation}>{children}</ApplicationLayout>
      </body>
    </html>
  )
}
