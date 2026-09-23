import clsx from 'clsx'

import { Button as NdsButton, type ButtonProps as NdsButtonProps } from '@nice-digital/nds-button'

import styles from './Button.module.scss'

import type { ComponentPropsWithoutRef } from 'react'

type NdsButtonVariant = NonNullable<NdsButtonProps['variant']>

export type ButtonVariant = NdsButtonVariant | 'link'

// NDS's string index signature causes Omit to widen inherited properties to unknown.
export type ButtonProps = Omit<NdsButtonProps, 'variant'> & {
  buttonType?: NdsButtonProps['buttonType']
  children: NdsButtonProps['children']
  className?: NdsButtonProps['className']
  variant?: ButtonVariant
}

type ButtonGroupProps = ComponentPropsWithoutRef<'div'>

export function Button({
  buttonType = 'button',
  className,
  variant = 'primary',
  ...props
}: ButtonProps) {
  if (variant === 'link') {
    return (
      <NdsButton
        {...props}
        buttonType={buttonType}
        className={clsx(styles.linkButton, className)}
      />
    )
  }

  return <NdsButton {...props} buttonType={buttonType} className={className} variant={variant} />
}

export function ButtonGroup({ children, className, ...props }: ButtonGroupProps) {
  return (
    <div {...props} className={clsx(styles.buttonGroup, className)}>
      {children}
    </div>
  )
}
