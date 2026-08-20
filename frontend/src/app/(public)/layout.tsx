import { inter, lora } from '@/app/fonts'
import { ApplicationLayout } from '@/components/Layout/ApplicationLayout'
import '@/styles/global.scss'

import type { ReactNode } from 'react'

export const metadata = {
  description: 'An essential first step to market access for medicines.',
  title: 'UK PharmaScan',
}

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html className={`${inter.variable} ${lora.variable}`} lang="en">
      <body>
        <ApplicationLayout>{children}</ApplicationLayout>
      </body>
    </html>
  )
}
