import clsx from 'clsx'

import { Alert as NdsAlert } from '@nice-digital/nds-alert'

import styles from './Alert.module.scss'

import type { ComponentPropsWithoutRef, ReactNode } from 'react'

export type AlertType = 'info' | 'caution' | 'error' | 'success'

export type AlertProps = Omit<ComponentPropsWithoutRef<'div'>, 'children'> & {
  children: ReactNode
  nonIntrusive?: boolean
  type?: AlertType
}

// Info and success alerts report on something the user has already done, so
// they're announced politely rather than interrupting whatever the screen
// reader is currently saying. Caution and error alerts need the user's
// attention straight away, so they keep the NDS default of `role="alert"`.
const nonIntrusiveByType: Record<AlertType, boolean> = {
  caution: false,
  error: false,
  info: true,
  success: true,
}

export function Alert({ children, className, nonIntrusive, type = 'info', ...rest }: AlertProps) {
  return (
    <div className={clsx(styles.wrapper, className)}>
      <NdsAlert nonIntrusive={nonIntrusive ?? nonIntrusiveByType[type]} type={type} {...rest}>
        {children}
      </NdsAlert>
    </div>
  )
}
