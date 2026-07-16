import Link from 'next/link'

export default function PortalNotFound() {
  return (
    <div>
      {/* Portal 404s are intentionally code-owned rather than Payload-managed. */}
      <h2>Page not found</h2>
      <p>The portal page you requested could not be found.</p>
      <Link href="/portal">Return to portal</Link>
    </div>
  )
}
