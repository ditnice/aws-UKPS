import clsx from 'clsx'
import Link, { type LinkProps } from 'next/link'

import { Alert as NdsAlert } from '@nice-digital/nds-alert'

import { Button } from '@/components/Button/Button'

import styles from './Alert.module.scss'

import type { ComponentPropsWithoutRef, ReactNode } from 'react'

export type AlertType = 'info' | 'caution' | 'error' | 'success'

export type AlertProps = Omit<ComponentPropsWithoutRef<'div'>, 'children'> & {
  children: ReactNode
  // Where to go when the alert is dismissed
  dismissHref?: LinkProps['href']
  dismissText?: string
  nonIntrusive?: boolean
  type?: AlertType
}

// nonIntrusive means the screen reader doesn't interrupt itself to read out the alert
// banner (i.e. announced politely). Caution and error alerts by default interrupt the screen reader
const nonIntrusiveByType: Record<AlertType, boolean> = {
  caution: false,
  error: false,
  info: true,
  success: true,
}

export function Alert({
  children,
  className,
  dismissHref,
  dismissText = 'Dismiss',
  nonIntrusive,
  type = 'info',
  ...rest
}: AlertProps) {
  return (
    <div className={clsx(styles.wrapper, className)}>
      <NdsAlert nonIntrusive={nonIntrusive ?? nonIntrusiveByType[type]} type={type} {...rest}>
        {children}
        {dismissHref && (
          <Button elementType={Link} href={dismissHref} variant="secondary">
            {dismissText}
          </Button>
        )}
      </NdsAlert>
    </div>
  )
}
