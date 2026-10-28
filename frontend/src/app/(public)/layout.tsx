import { ApplicationLayout } from '@/components/Layout/ApplicationLayout'
import { inter, lora } from '@/styles/fonts'
import '@/styles/global.scss'

import type { ReactNode } from 'react'

export const metadata = {
  description: 'An essential first step to market access for medicines.',
  title: 'UK PharmaScan',
}

export default function PublicLayout({ children }: { children: ReactNode }) {
  return (
    <html className={`${inter.variable} ${lora.variable}`} lang="en">
      <body>
        <ApplicationLayout
          links={[
            { href: '/', label: 'Home' },
            {
              href: '/register',
              label: (
                <>
                  Request a user
                  <br />
                  account
                </>
              ),
            },
          ]}
        >
          {children}
        </ApplicationLayout>
      </body>
    </html>
  )
}
