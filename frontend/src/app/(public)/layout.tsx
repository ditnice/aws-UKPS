import React from 'react'

import { ApplicationLayout } from '@/components/ApplicationLayout'
import '@/styles/global.scss'

import './styles.css'

export const metadata = {
  description: 'A blank template using Payload in a Next.js app.',
  title: 'Payload Blank Template',
}

export default async function RootLayout(props: { children: React.ReactNode }) {
  const { children } = props

  return (
    <html lang="en">
      <body>
        <ApplicationLayout>{children}</ApplicationLayout>
      </body>
    </html>
  )
}
