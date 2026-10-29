'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import React from 'react'

import styles from './NavLinks.module.scss'

export type NavLinkItem = {
  href: string
  label: string | React.ReactElement
}

export type NavLinksProps = {
  links: NavLinkItem[]
}

function matchesPath(pathname: string, href: string) {
  if (href === '/') {
    return pathname === href
  }
  return pathname === href || pathname.startsWith(href)
}

// Only the most specific matching link is active.
function getActiveHref(pathname: string, links: NavLinkItem[]): string | undefined {
  return links
    .map(({ href }) => href)
    .filter((href) => matchesPath(pathname, href))
    .sort((a, b) => b.length - a.length)[0]
}

export function NavLinks({ links }: NavLinksProps) {
  const pathname = usePathname()
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
