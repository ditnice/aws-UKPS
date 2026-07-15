import styles from '../page.module.scss'

import type { ReactNode } from 'react'

type ExampleProps = {
  children: ReactNode
  dark?: boolean
  fullWidth?: boolean
  title: string
}

export function Example({ children, dark = false, fullWidth = false, title }: ExampleProps) {
  return (
    <div
      className={`${styles.example}${dark ? ` ${styles.exampleDark}` : ''}${fullWidth ? ` ${styles.exampleFullWidth}` : ''}`}
    >
      <h2 className={styles.exampleTitle}>{title}</h2>
      <div className={styles.exampleBody}>{children}</div>
    </div>
  )
}
