import clsx from 'clsx'

import styles from './Nav.module.scss'
import { NavLinks } from './NavLinks/NavLinks'

export type NavProps = {
  isExpanded: boolean
  organisationId?: number
}

export function Nav({ isExpanded, organisationId }: NavProps) {
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
            <NavLinks organisationId={organisationId} />
          </div>
        </nav>
      </div>
    </>
  )
}
