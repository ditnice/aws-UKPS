'use client'

import Image from 'next/image'
import Link from 'next/link'
import { useState, type ReactElement } from 'react'

import { UserRole } from '@/client/generated'

import { Account } from './Account/Account'
import styles from './Header.module.scss'
import { Nav } from './Nav/Nav'
import { NavLinkItem } from './Nav/NavLinks/NavLinks'
import { SkipLink } from './SkipLink/SkipLink'

export type UserOrganisationContext = {
  organisationId: number
  organisationName: string
  hasMultipleOrganisations: boolean
  userRole: UserRole
}

export type HeaderProps = {
  skipLinkId: string
  links: NavLinkItem[]
  headerExtension?: ReactElement
}

export function Header({ skipLinkId, links, headerExtension }: HeaderProps) {
  const [isExpanded, setIsExpanded] = useState(false)

  const handleMobileMenuBtnClick = () => {
    setIsExpanded((currentIsExpanded) => !currentIsExpanded)
  }

  return (
    <>
      <div className={styles.header} data-component="header" id="top">
        <header aria-label="Site header">
          <ul className={styles.a11yLinks} aria-label="Accessibility links">
            <li>
              <SkipLink to={`#${skipLinkId}`}>Skip to content</SkipLink>
            </li>
            <li>
              <SkipLink to="/accessibility">Accessibility help</SkipLink>
            </li>
          </ul>
          <div className={styles.container}>
            <Link
              href="/"
              aria-label="NICE: National Institute for Health and Care Excellence homepage"
              className={styles.home}
            >
              <Image
                src="/assets/ukps-logo.png"
                alt=""
                className={styles.icon}
                width={750}
                height={82}
                priority
              />
            </Link>
            <div className={styles.wrapper}>
              <button
                className={styles.mobileMenuBtn}
                id="header-menu-button"
                type="button"
                aria-controls="header-menu"
                aria-expanded={isExpanded}
                aria-haspopup="menu"
                aria-label={isExpanded ? 'Close site menu' : 'Expand site menu'}
                onClick={handleMobileMenuBtnClick}
              >
                {isExpanded ? 'Close' : 'Menu'}
              </button>
              <div className={styles.account}>
                <Account />
              </div>
            </div>
          </div>
          <Nav isExpanded={isExpanded} links={links} />
        </header>
      </div>
      {headerExtension}
    </>
  )
}
