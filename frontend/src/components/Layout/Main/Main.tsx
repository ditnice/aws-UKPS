import clsx from 'clsx'
import { type ComponentPropsWithoutRef } from 'react'

import { BackToTop } from './BackToTop/BackToTop'
import styles from './Main.module.scss'

type MainProps = ComponentPropsWithoutRef<'main'> & {
  withPadding?: boolean
}

export function Main({ children, className, withPadding = true, ...rest }: MainProps) {
  return (
    <main
      className={clsx([styles.main, className], withPadding && styles.withPadding)}
      {...rest}
      data-component="main"
    >
      {children}
      <BackToTop />
    </main>
  )
}
