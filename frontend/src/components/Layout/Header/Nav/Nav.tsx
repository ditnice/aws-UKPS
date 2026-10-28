import clsx from 'clsx'

import styles from './Nav.module.scss'
import { NavLinkItem, NavLinks } from './NavLinks/NavLinks'

export type NavProps = {
  isExpanded: boolean
  links: NavLinkItem[]
}

export function Nav({ isExpanded, links }: NavProps) {
  return (
    <>
      <div
        id="header-menu"
        className={clsx(styles.wrapper, {
          [styles.wrapperExpanded]: isExpanded,
        })}
      >
        <nav className={styles.nav} aria-label="primary navigation">
          <div className={styles.menuWrapper}>
            <NavLinks links={links} />
          </div>
        </nav>
      </div>
    </>
  )
}
