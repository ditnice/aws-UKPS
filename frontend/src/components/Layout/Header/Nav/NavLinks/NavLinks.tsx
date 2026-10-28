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

function isActiveLink(pathname: string, href: string) {
  const hrefPath = href.split(/[?#]/)[0]

  if (hrefPath === '/') {
    return pathname === hrefPath
  }

  return pathname === hrefPath
}

export function NavLinks({ links }: NavLinksProps) {
  const pathname = usePathname()
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
