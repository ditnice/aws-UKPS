import styles from './BackLink.module.scss'

import type { ComponentPropsWithoutRef, ReactNode } from 'react'

export type BackLinkVariant = 'default' | 'inverse'

export type BackLinkProps = Omit<ComponentPropsWithoutRef<'a'>, 'children'> & {
  children?: ReactNode
  href: string
  variant?: BackLinkVariant
}

export function BackLink({
  children = 'Back',
  className,
  variant = 'default',
  ...rest
}: BackLinkProps) {
  const rootClassName = [
    styles['back-link'],
    variant === 'inverse' && styles['back-link--inverse'],
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <a className={rootClassName} data-component="back-link" {...rest}>
      {children}
    </a>
  )
}
