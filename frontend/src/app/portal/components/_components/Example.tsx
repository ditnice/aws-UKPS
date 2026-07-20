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
      className={`${styles.example}${dark ? ` ${styles['example-dark']}` : ''}${fullWidth ? ` ${styles['example-full-width']}` : ''}`}
    >
      <h2 className={styles['example-title']}>{title}</h2>
      <div className={styles['example-body']}>{children}</div>
    </div>
  )
}
