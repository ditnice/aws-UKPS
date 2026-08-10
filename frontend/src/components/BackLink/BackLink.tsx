import clsx from 'clsx'
import Link, { type LinkProps } from 'next/link'

import styles from './BackLink.module.scss'

import type { AnchorHTMLAttributes, ReactNode } from 'react'

export type BackLinkVariant = 'default' | 'inverse'

export type BackLinkProps = Omit<AnchorHTMLAttributes<HTMLAnchorElement>, 'children' | 'href'> &
  LinkProps & {
    children?: ReactNode
    variant?: BackLinkVariant
  }

export function BackLink({
  children = 'Back',
  className,
  variant = 'default',
  ...rest
}: BackLinkProps) {
  const rootClassName = clsx(
    styles['back-link'],
    variant === 'inverse' && styles['back-link--inverse'],
    className,
  )

  return (
    <Link className={rootClassName} data-component="back-link" {...rest}>
      {children}
    </Link>
  )
}
