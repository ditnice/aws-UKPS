import styles from './BackLink.module.scss'

import type { ComponentPropsWithoutRef, ReactNode } from 'react'

export type BackLinkProps = Omit<ComponentPropsWithoutRef<'a'>, 'children'> & {
  children?: ReactNode
  href: string
}

export function BackLink({ children = 'Back', className, ...rest }: BackLinkProps) {
  return (
    <a
      className={`${styles['back-link']}${className ? ` ${className}` : ''}`}
      data-component="back-link"
      {...rest}
    >
      {children}
    </a>
  )
}
