import clsx from 'clsx'

import styles from './Details.module.scss'

import type { ComponentPropsWithoutRef, ReactNode } from 'react'

export type DetailsProps = Omit<ComponentPropsWithoutRef<'details'>, 'children' | 'title'> & {
  children: ReactNode
  summary: ReactNode
}

export function Details({ children, className, summary, ...rest }: DetailsProps) {
  return (
    <details className={clsx(styles.details, className)} data-component="details" {...rest}>
      <summary className={styles.summary}>
        <span className={styles['summary-text']}>{summary}</span>
      </summary>
      <div className={styles.text}>{children}</div>
    </details>
  )
}
