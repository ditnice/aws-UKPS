import clsx from 'clsx'

import styles from './Nav.module.scss'
import { NavLinks } from './NavLinks/NavLinks'

export type NavProps = {
  isExpanded: boolean
}

export function Nav({ isExpanded }: NavProps) {
  return (
    <>
      <div
        id="header-menu"
        className={clsx(
          styles.wrapper,
          {
            [styles.wrapperExpanded]: isExpanded,
          },
          //   {
          //     [styles.wrapperWithSubLinks]: subLinks,
          //   },
        )}
        // ref={clickOutsideRef}
      >
        <nav
          className={styles.nav}
          aria-label="primary navigation"
          data-tracking="Primary navigation"
        >
          <div className={styles.menuWrapper}>
            <NavLinks
            //   skipLinkId={skipLinkId}
            //   servicesToDisplay={servicesToDisplay}
            //   currentService={service}
            //   subLinks={subLinks}
            //   onNavigating={onNavigating}
            />
          </div>
        </nav>
        {/* TODO: when is this used?
        {accountsLinksArray && (
          <nav aria-label="My account" className={clsx(styles.nav, styles.myAccount)}>
            {accountsLinksArray.length > 1 && (
              <h2 className={styles.myAccountHeading}>My account</h2>
            )}
            <div className={styles.menuWrapper}>
              <ul className={styles.menuList}>
                {accountsLinksArray.map(({ href, text }) => (
                  <li key={href}>
                    <a href={href} className={styles.link} onClick={handleAccountNavItemClick}>
                      {text}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          </nav>
        )} */}
      </div>
    </>
  )
}
