import { ApplicationLayout } from '@/components/ApplicationLayout'
import '@/styles/global.scss'

import type { ReactNode } from 'react'

export default function PortalLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>
        <ApplicationLayout>{children}</ApplicationLayout>
      </body>
    </html>
  )
}
