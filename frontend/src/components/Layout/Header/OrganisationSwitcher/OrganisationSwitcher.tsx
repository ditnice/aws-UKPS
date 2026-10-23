import Link from 'next/link'

import { selectOrganisationPath } from '@/lib/auth/routing'

import styles from './OrganisationSwitcher.module.scss'

export type OrganisationSwitcherProps = {
  organisationName: string
}

// Based on the MOJ organisation switcher pattern:
// https://design-patterns.service.justice.gov.uk/components/organisation-switcher/
export function OrganisationSwitcher({ organisationName }: OrganisationSwitcherProps) {
  return (
    <nav className={styles.organisationNav} aria-label="Organisation switcher">
      <div className={styles.title}>{organisationName}</div>
      <Link href={selectOrganisationPath} className={styles.link}>
        Change organisation
      </Link>
    </nav>
  )
}
