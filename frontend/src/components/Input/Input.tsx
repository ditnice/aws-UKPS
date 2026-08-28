import clsx from 'clsx'

import '@nice-digital/nds-input/scss/input.scss'

import type { ComponentPropsWithoutRef, CSSProperties, Ref } from 'react'

export type InputFixedWidth = 2 | 3 | 4 | 5 | 10 | 20 | 30

export type InputFluidWidth =
  'full' | 'three-quarters' | 'two-thirds' | 'one-half' | 'one-third' | 'one-quarter'

export type InputWidth = InputFixedWidth | InputFluidWidth

export type InputProps = Omit<ComponentPropsWithoutRef<'input'>, 'className'> & {
  className?: string
  error?: boolean
  errorMessage?: string
  hint?: string
  inputRef?: Ref<HTMLInputElement>
  label: string | null
  name: string
  /**
   * Fixed widths (2, 3, 4, 5, 10, 20, 30) constrain the input to a character-based
   * max-width. Fluid widths resize the input as a percentage of its container.
   * Matches the GOV.UK Design System text input width scale.
   */
  width?: InputWidth
}

export const inputWidthStyles: Record<InputWidth, CSSProperties> = {
  2: { maxWidth: '2.75em' },
  3: { maxWidth: '3.75em' },
  4: { maxWidth: '4.5em' },
  5: { maxWidth: '5.5em' },
  10: { maxWidth: '11.5em' },
  20: { maxWidth: '20.5em' },
  30: { maxWidth: '29.5em' },
  full: { width: '100%' },
  'three-quarters': { width: '75%' },
  'two-thirds': { width: '66.6667%' },
  'one-half': { width: '50%' },
  'one-third': { width: '33.3333%' },
  'one-quarter': { width: '25%' },
}

export function Input({
  'aria-describedby': describedBy,
  'aria-invalid': ariaInvalid,
  className,
  error,
  errorMessage,
  hint,
  id,
  inputRef,
  label,
  name,
  style,
  type = 'text',
  width,
  ...rest
}: InputProps) {
  const inputId = id ?? name
  const hintId = hint ? `${inputId}-hint` : undefined
  const errorId = error ? `${inputId}-error` : undefined
  const ariaDescribedBy = [describedBy, hintId, errorId].filter(Boolean).join(' ') || undefined
  const inputStyle = width ? { ...style, ...inputWidthStyles[width] } : style

  return (
    <div className={clsx('input', error && 'input--error', className)} data-component="input">
      {label && (
        <label className="input__label" htmlFor={inputId}>
          {label}
        </label>
      )}
      {hint && (
        <p className="input__hint" id={hintId}>
          {hint}
        </p>
      )}
      {error && (
        <p className="input__error" id={errorId}>
          <span className="visually-hidden">Error:</span> {errorMessage}
        </p>
      )}
      <input
        {...rest}
        aria-describedby={ariaDescribedBy}
        aria-invalid={error ? true : ariaInvalid}
        className="input__input"
        id={inputId}
        name={name}
        ref={inputRef}
        style={inputStyle}
        type={type}
      />
    </div>
  )
}
