import { Children, isValidElement } from 'react'

import styles from './SummaryCard.module.scss'

import type { ReactNode } from 'react'

export type SummaryCardProps = {
  /** Links on the title line, e.g. `SummaryListAction`s such as "Edit section". */
  actions?: ReactNode
  /** Usually a `SummaryList`, with any intro text before it. */
  children: ReactNode
  /** Shown on the title line, to the left of any actions, e.g. a problem `Tag`. */
  tag?: ReactNode
  title: ReactNode
  /** Defaults to 2: the page heading is the h1. Nested cards, like trials, pass 3. */
  titleHeadingLevel?: 2 | 3 | 4 | 5 | 6
}

/** Grey hint text inside a card, below a dividing line, such as why a task has no answers. */
export function SummaryCardHint({ children }: { children: ReactNode }) {
  return <p className={styles.hint}>{children}</p>
}

/** A titled box around a summary list or other content, with optional links on the title line. */
export function SummaryCard({
  actions,
  children,
  tag,
  title,
  titleHeadingLevel = 2,
}: SummaryCardProps) {
  const TitleHeading = `h${titleHeadingLevel}` as const
  const actionElements = Children.toArray(actions).filter((action) => isValidElement(action))
  const actionLinks = (
    <>
      {actionElements.length === 1 && <div className={styles.actions}>{actionElements[0]}</div>}
      {actionElements.length > 1 && (
        <ul className={styles.actions}>
          {actionElements.map((action) => (
            <li className={styles.action} key={action.key}>
              {action}
            </li>
          ))}
        </ul>
      )}
    </>
  )

  return (
    <div className={styles.summaryCard}>
      <div className={styles.header}>
        <TitleHeading className={styles.title}>{title}</TitleHeading>
        {tag ? (
          <div className={styles.tagAndActions}>
            {tag}
            {actionLinks}
          </div>
        ) : (
          actionLinks
        )}
      </div>
      {children}
    </div>
  )
}
