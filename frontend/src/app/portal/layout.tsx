import type { ReactNode } from 'react'

export default function PortalLayout({ children }: { children: ReactNode }) {
  return (
    <div>
      <nav>{/* Authenticated nav */}</nav>
      <main>{children}</main>
    </div>
  )
}
