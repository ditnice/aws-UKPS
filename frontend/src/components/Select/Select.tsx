import clsx from 'clsx'

import { inputWidthStyles } from '@/components/Input/Input'
import type { InputWidth } from '@/components/Input/Input'

import styles from './Select.module.scss'

import type { ComponentPropsWithoutRef, Ref } from 'react'

export type SelectOptionProps = ComponentPropsWithoutRef<'option'>

export function SelectOption({ children, ...rest }: SelectOptionProps) {
  return <option {...rest}>{children}</option>
}

export type SelectProps = Omit<ComponentPropsWithoutRef<'select'>, 'multiple'> & {
  error?: boolean
  errorMessage?: string
  hint?: string
  label: string | null
  name: string
  selectRef?: Ref<HTMLSelectElement>
  /**
   * Fixed widths (2, 3, 4, 5, 10, 20, 30) constrain the select to a character-based
   * max-width. Fluid widths resize the select as a percentage of its container.
   * Matches the GOV.UK Design System width scale - see @/components/Input/Input.
   */
  width?: InputWidth
}

export function Select({
  'aria-describedby': describedBy,
  children,
  className,
  error,
  errorMessage,
  hint,
  id,
  label,
  name,
  selectRef,
  style,
  width,
  ...rest
}: SelectProps) {
  const selectId = id ?? name
  const hintId = hint ? `${selectId}-hint` : undefined
  const errorId = error ? `${selectId}-error` : undefined
  const ariaDescribedBy = [describedBy, hintId, errorId].filter(Boolean).join(' ') || undefined
  const selectStyle = width ? { ...style, ...inputWidthStyles[width] } : style

  return (
    <div className={clsx(styles.select, className)} data-component="select">
      {label && (
        <label className={styles.label} htmlFor={selectId}>
          {label}
        </label>
      )}
      {hint && (
        <div className={styles.hint} id={hintId}>
          {hint}
        </div>
      )}
      {error && (
        <p className={styles.error} id={errorId}>
          <span className="visually-hidden">Error:</span> {errorMessage}
        </p>
      )}
      <select
        {...rest}
        aria-describedby={ariaDescribedBy}
        className={clsx(styles.field, error && styles.fieldError)}
        id={selectId}
        name={name}
        ref={selectRef}
        style={selectStyle}
      >
        {children}
      </select>
    </div>
  )
}
