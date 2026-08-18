import Link from 'next/link'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { componentDefinitions } from './_data/components'

export const metadata = {
  description: 'NICE Design System React component examples for the UKPS portal.',
  title: 'NICE Design System components',
}

export default function ComponentsPage() {
  return (
    <>
      <PageHeader
        backLink={<BackLink href={`/portal`}>Back</BackLink>}
        heading="NICE Design System components"
      />
      <p>Select a component to view its variants.</p>
      <ul>
        {componentDefinitions.map(({ label, slug }) => (
          <li key={slug}>
            <Link href={`/portal/components/${slug}`} prefetch={false}>
              {label}
            </Link>
          </li>
        ))}
      </ul>
    </>
  )
}
