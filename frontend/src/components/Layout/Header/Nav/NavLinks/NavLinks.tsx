'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'

import styles from './NavLinks.module.scss'

type NavLinkItem = {
  href: string
  label: string
}

export type NavLinksProps = {
  rootLinks?: NavLinkItem[]
  portalLinks?: NavLinkItem[]
  organisationId?: number
}

const defaultRootLinks: NavLinkItem[] = [
  { href: '/', label: 'Home' },
  { href: '/about-us', label: 'About' },
]

// Organisation links are only shown once the user has a current organisation.
function getDefaultPortalLinks(organisationId: number | undefined): NavLinkItem[] {
  const organisationLinks: NavLinkItem[] =
    organisationId === undefined
      ? []
      : [
          {
            href: `/portal/organisations/${organisationId}/records`,
            label: 'View and manage records',
          },
          { href: `/portal/organisations/${organisationId}`, label: 'Manage my organisation' },
        ]

  return [
    { href: '/portal', label: 'Dashboard' },
    { href: '/portal/components', label: 'Components' },
    ...organisationLinks,
    { href: '/portal/user/me', label: 'Manage my details' },
  ]
}

function isPortalPath(pathname: string) {
  return pathname === '/portal' || pathname.startsWith('/portal/')
}

// Section roots would otherwise match every page within them.
const exactMatchHrefs = new Set(['/', '/portal'])

function matchesPath(pathname: string, href: string) {
  if (exactMatchHrefs.has(href)) {
    return pathname === href
  }

  return pathname === href || pathname.startsWith(`${href}/`)
}

// Only the most specific matching link is active.
function getActiveHref(pathname: string, links: NavLinkItem[]): string | undefined {
  return links
    .map(({ href }) => href)
    .filter((href) => matchesPath(pathname, href))
    .sort((a, b) => b.length - a.length)[0]
}

export function NavLinks({
  rootLinks = defaultRootLinks,
  portalLinks,
  organisationId,
}: NavLinksProps) {
  const pathname = usePathname()
  const links = isPortalPath(pathname)
    ? (portalLinks ?? getDefaultPortalLinks(organisationId))
    : rootLinks
  const activeHref = getActiveHref(pathname, links)

  return (
    <ul className={styles.menuList} aria-labelledby="header-menu-button">
      {links.map(({ href, label }) => (
        <li key={href}>
          <Link
            href={href}
            className={styles.link}
            aria-current={href === activeHref ? 'page' : undefined}
          >
            {label}
          </Link>
        </li>
      ))}
    </ul>
  )
}
