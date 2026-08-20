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
}

const defaultRootLinks: NavLinkItem[] = [
  { href: '/', label: 'Home' },
  { href: '/about-us', label: 'About' },
]

const defaultPortalLinks: NavLinkItem[] = [
  { href: '/portal', label: 'Dashboard' },
  { href: '/portal/components', label: 'Components' },
]

function isPortalPath(pathname: string) {
  return pathname === '/portal' || pathname.startsWith('/portal/')
}

function isActiveLink(pathname: string, href: string) {
  if (href === '/') {
    return pathname === href
  }

  return pathname === href || pathname.startsWith(`${href}/`)
}

export function NavLinks({
  rootLinks = defaultRootLinks,
  portalLinks = defaultPortalLinks,
}: NavLinksProps) {
  const pathname = usePathname()
  const links = isPortalPath(pathname) ? portalLinks : rootLinks

  return (
    <ul className={styles.menuList} aria-labelledby="header-menu-button">
      {links.map(({ href, label }) => (
        <li key={href}>
          <Link
            href={href}
            className={styles.link}
            aria-current={isActiveLink(pathname, href) ? 'page' : undefined}
          >
            {label}
          </Link>
        </li>
      ))}
    </ul>
  )
}
