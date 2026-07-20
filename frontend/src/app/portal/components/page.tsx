import Link from 'next/link'

import { componentDefinitions } from './_data/components'

export const metadata = {
  description: 'NICE Design System React component examples for the UKPS portal.',
  title: 'NICE Design System components',
}

export default function ComponentsPage() {
  return (
    <>
      <h1>NICE Design System components</h1>
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
