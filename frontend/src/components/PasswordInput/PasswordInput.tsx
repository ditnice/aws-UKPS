'use client'

import { useState } from 'react'

import { Button } from '@nice-digital/nds-button'

import { inputWidthStyles } from '@/components/Input/Input'
import type { InputWidth } from '@/components/Input/Input'

import styles from './PasswordInput.module.scss'

import type { ComponentPropsWithoutRef, Ref } from 'react'

export type PasswordInputProps = Omit<
  ComponentPropsWithoutRef<'input'>,
  'defaultValue' | 'type'
> & {
  defaultValue?: string
  error?: boolean
  errorMessage?: string
  hint?: string
  inputRef?: Ref<HTMLInputElement>
  label: string | null
  name: string
  /**
   * Fixed widths (2, 3, 4, 5, 10, 20, 30) constrain the input to a character-based
   * max-width. Fluid widths resize the input as a percentage of its container.
   * Matches the GOV.UK Design System text input width scale - see @/components/Input/Input.
   */
  width?: InputWidth
}

export function PasswordInput({
  autoComplete = 'current-password',
  className,
  error,
  errorMessage,
  hint,
  inputRef,
  label,
  name,
  style,
  width,
  ...rest
}: PasswordInputProps) {
  const [visible, setVisible] = useState(false)

  function handleToggleClick() {
    setVisible((currentlyVisible) => !currentlyVisible)
  }

  const rootClassName = [styles.input, className].filter(Boolean).join(' ')
  const fieldClassName = [styles.field, error && styles.fieldError].filter(Boolean).join(' ')
  const fieldStyle = width ? { ...style, ...inputWidthStyles[width] } : style

  return (
    <div className={rootClassName} data-component="input">
      {label && (
        <label className={styles.label} htmlFor={name}>
          {label}
        </label>
      )}
      {hint && <p className={styles.hint}>{hint}</p>}
      {error && <p className={styles.error}>{errorMessage}</p>}
      <div className={styles.wrapper}>
        <input
          {...rest}
          autoCapitalize="none"
          autoComplete={autoComplete}
          className={fieldClassName}
          id={name}
          name={name}
          ref={inputRef}
          spellCheck={false}
          style={fieldStyle}
          type={visible ? 'text' : 'password'}
        />
        <div aria-live="polite" className="visually-hidden">
          {visible ? 'Your password is visible' : 'Your password is hidden'}
        </div>
        <Button
          aria-controls={name}
          aria-label={visible ? 'Hide password' : 'Show password'}
          className={styles.toggle}
          onClick={handleToggleClick}
          variant="inverse"
        >
          {visible ? 'Hide' : 'Show'}
        </Button>
      </div>
    </div>
  )
}
