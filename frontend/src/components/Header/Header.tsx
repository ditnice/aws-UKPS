'use client'

import Image from 'next/image'
import { useState } from 'react'

import styles from './Header.module.scss'
import { Nav } from './Nav/Nav'
import { SkipLink } from './SkipLink/SkipLink'

export type HeaderProps = {
  skipLinkId: string
}

export function Header({ skipLinkId }: HeaderProps) {
  const [isExpanded, setIsExpanded] = useState(false)

  const handleMobileMenuBtnClick = () => {
    setIsExpanded((currentIsExpanded) => !currentIsExpanded)
  }

  return (
    <>
      <div className={styles.header} data-tracking="Global nav" id="top">
        <header aria-label="Site header">
          <ul className={styles.a11yLinks} aria-label="Accessibility links">
            <li>
              <SkipLink to={`#${skipLinkId}`}>Skip to content</SkipLink>
            </li>
            <li>
              <SkipLink to="https://www.nice.org.uk/accessibility">Accessibility help</SkipLink>
            </li>
          </ul>
          <div className={styles.container}>
            <a
              href="https://www.nice.org.uk/"
              aria-label="NICE: National Institute for Health and Care Excellence homepage"
              className={styles.home}
              // onClick={handleLogoClick} TODO - make this do something
              data-tracking="Logo"
            >
              <Image
                src="/assets/ukps-logo.png"
                alt=""
                className={styles.icon}
                width={750}
                height={82}
                priority
              />
            </a>
            <div className={styles.wrapper}>
              {/* <div className={styles.search}>
                {search && (
                  <Search skipLinkId={skipLinkId} onNavigating={onNavigating} {...search} />
                )}
              </div> */}
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
              {/* {auth !== false && (
                <div className={styles.account}>
                  <Account
                    onLoginStatusChecked={handleLoginStatusChecked}
                    isLoggedIn={isLoggedIn}
                    accountsData={accountsData}
                    {...auth}
                  />
                </div>
              )} */}
            </div>
          </div>
          <Nav
            // skipLinkId={skipLinkId}
            // service={service}
            isExpanded={isExpanded}
            // accountsLinks={accountsData && accountsData.links}
            // onNavigating={onNavigating}
            // additionalSubMenuItems={additionalSubMenuItems}
          />
        </header>
      </div>
    </>
  )
}
