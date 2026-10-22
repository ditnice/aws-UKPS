import Link from 'next/link'

import { PageHeader } from '@/components/PageHeader/PageHeader'

export default function PortalDashboard() {
  return (
    <>
      <PageHeader heading="Your dashboard" />

      <p>Development quick reference links:</p>
      <ul>
        <li>
          <Link href="/portal/notfound">Not found example</Link>
        </li>
        <li>
          <Link href="/portal/components">Components</Link>
        </li>
        <li>
          <Link href="/portal/organisations/2">Organisation 2 manage org</Link>
        </li>
        <li>
          <Link href="/portal/organisations/2/records">Organisation 2 records dashboard</Link>
        </li>
      </ul>
    </>
  )
}
