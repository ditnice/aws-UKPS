import Link from 'next/link'

export default function PortalDashboard() {
  return (
    <>
      <h1>Your dashboard</h1>
      <ul>
        <li>
          <Link href="/portal/notfound">Not found example</Link>
        </li>
        <li>
          <Link href="/portal/components">Components</Link>
        </li>
        <li>
          <Link href="/portal/organisations/1">Organisation 1 example</Link>
        </li>
      </ul>
    </>
  )
}
