import clsx from 'clsx'

import styles from './Badge.module.scss'

import type { ComponentPropsWithoutRef, ReactNode } from 'react'

export type BadgeColour = 'blue' | 'purple' | 'bright-purple' | 'red' | 'green' | 'black' | 'grey'

export type BadgeProps = Omit<ComponentPropsWithoutRef<'span'>, 'children'> & {
  children: ReactNode
  colour?: BadgeColour
  /** Additional context announced to screen reader users only, e.g. "Case status:" */
  label?: string
  large?: boolean
}

export function Badge({
  children,
  className,
  colour = 'blue',
  label,
  large = false,
  ...rest
}: BadgeProps) {
  return (
    <span
      className={clsx(styles.badge, styles[colour], large && styles.large, className)}
      data-component="badge"
      {...rest}
    >
      {label && <span className="visually-hidden">{label} </span>}
      {children}
    </span>
  )
}
