import Link from 'next/link'

import { PageHeader } from '@/components/PageHeader/PageHeader'

export default function PortalNotFound() {
  return (
    <div>
      {/* Portal 404s are intentionally code-owned rather than Payload-managed. */}
      <PageHeader heading="Page not found"></PageHeader>
      <p>The portal page you requested could not be found.</p>
      <Link href="/portal">Return to portal</Link>
    </div>
  )
}
