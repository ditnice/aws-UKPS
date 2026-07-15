import { notFound } from 'next/navigation'

// Keep unmatched portal paths, such as /portal/does-not-exist or /portal/foo/bar,
// inside the portal route tree so they render app/portal/not-found.tsx.
export default function PortalCatchAllPage() {
  notFound()
}
