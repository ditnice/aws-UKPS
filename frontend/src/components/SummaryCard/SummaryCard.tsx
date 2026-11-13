import clsx from 'clsx'

import styles from './SummaryCard.module.scss'

import type { ReactNode } from 'react'

export type SummaryCardProps = {
  /* Links in the header, e.g. `SummaryListAction`s such as "Edit section". */
  actions?: ReactNode
  children: ReactNode
  className?: string
  title: ReactNode
  /* Defaults to 2: the page heading is the h1. Nested cards, like trials, pass 3. */
  titleHeadingLevel?: 2 | 3 | 4 | 5 | 6
}

export function SummaryCard({
  actions,
  children,
  className,
  title,
  titleHeadingLevel = 2,
}: SummaryCardProps) {
  const TitleHeading = `h${titleHeadingLevel}` as const

  return (
    <div className={clsx(styles.summaryCard, className)}>
      <div className={styles.header}>
        <TitleHeading className={styles.title}>{title}</TitleHeading>
        {actions && <div className={styles.actions}>{actions}</div>}
      </div>
      {children}
    </div>
  )
}
