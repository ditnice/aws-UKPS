import styles from './SummaryCard.module.scss'

import type { ReactNode } from 'react'

export type SummaryCardProps = {
  /* Links on the title line, e.g. `SummaryListAction`s such as "Edit section". */
  actions?: ReactNode
  /* Usually a `SummaryList`, with any intro text before it. */
  children: ReactNode
  title: ReactNode
  /* Defaults to 2: the page heading is the h1. Nested cards, like trials, pass 3. */
  titleHeadingLevel?: 2 | 3 | 4 | 5 | 6
}

/** A titled box around a summary list or other content, with optional links on the title line. */
export function SummaryCard({ actions, children, title, titleHeadingLevel = 2 }: SummaryCardProps) {
  const TitleHeading = `h${titleHeadingLevel}` as const

  return (
    <div className={styles.summaryCard}>
      <div className={styles.header}>
        <TitleHeading className={styles.title}>{title}</TitleHeading>
        {actions && <div className={styles.actions}>{actions}</div>}
      </div>
      {children}
    </div>
  )
}
