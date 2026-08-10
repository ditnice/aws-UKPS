import clsx from 'clsx'

import { Button as NdsButton, type ButtonProps as NdsButtonProps } from '@nice-digital/nds-button'

import styles from './Button.module.scss'

type NdsButtonVariant = NonNullable<NdsButtonProps['variant']>

export type ButtonVariant = NdsButtonVariant | 'link'

export type ButtonProps = Omit<NdsButtonProps, 'variant'> & {
  variant?: ButtonVariant
}

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
