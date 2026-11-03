'use client'

import clsx from 'clsx'
import { createContext, useContext, useEffect, useEffectEvent, useRef, useState } from 'react'

import '@nice-digital/nds-form-group/scss/form-group.scss'
import '@nice-digital/nds-radio/scss/radio.scss'

import styles from './Radios.module.scss'

import type { ComponentPropsWithoutRef, ElementType, FocusEventHandler, ReactNode } from 'react'

type RadiosContextValue = {
  error: boolean
  describedBy?: string
  inline: boolean
  name: string
  onBlur?: FocusEventHandler<HTMLInputElement>
  select: (value: string) => void
  selectedValue: string | null
}

const RadiosContext = createContext<RadiosContextValue | null>(null)

function useRadiosContext(): RadiosContextValue {
  const context = useContext(RadiosContext)
  if (!context) {
    throw new Error('Radio must be rendered inside Radios')
  }
  return context
}

export type RadiosProps = {
  'aria-describedby'?: string
  children: ReactNode
  className?: string
  /** Initial selection when uncontrolled. */
  defaultValue?: string
  /** Error message shown above the options; also styles every option as invalid. */
  error?: string
  /** Renders the legend inside a heading of this level, e.g. on a single-question page. */
  headingLevel?: number
  hint?: ReactNode
  /**
   * Lays the options out side by side. Conditional content is not revealed for inline radios,
   * following the GOV.UK Design System guidance.
   */
  inline?: boolean
  legend: string
  name: string
  onBlur?: FocusEventHandler<HTMLInputElement>
  onChange?: (value: string) => void
  /** Current selection when controlled; `null` means nothing is selected. */
  value?: string | null
}

/**
 * A semantic group of radios using the NDS form group and radio styles. Individual options can
 * conditionally reveal a related question when selected - see `Radio`'s `conditional` prop and
 * https://design-system.service.gov.uk/components/radios/#conditionally-revealing-a-related-question
 */
export function Radios({
  'aria-describedby': describedBy,
  children,
  className,
  defaultValue,
  error,
  headingLevel,
  hint,
  inline = false,
  legend,
  name,
  onBlur,
  onChange,
  value,
}: RadiosProps) {
  const [uncontrolledValue, setUncontrolledValue] = useState<string | null>(defaultValue ?? null)
  const isControlled = value !== undefined
  const selectedValue = isControlled ? value : uncontrolledValue
  const fieldsetRef = useRef<HTMLFieldSetElement>(null)
  const hintId = hint ? `${name}-hint` : undefined
  const errorId = error ? `${name}-error` : undefined
  const descriptions = [describedBy, hintId, errorId].filter(Boolean).join(' ') || undefined
  const Heading = (
    headingLevel && Number.isInteger(headingLevel) && headingLevel >= 1 && headingLevel <= 6
      ? `h${headingLevel}`
      : 'span'
  ) as ElementType

  const restoreAfterReset = useEffectEvent((event: Event) => {
    const fieldset = fieldsetRef.current
    if (event.defaultPrevented || !fieldset?.isConnected) return
    const resetValue = isControlled ? value : (defaultValue ?? null)
    if (!isControlled) setUncontrolledValue(resetValue ?? null)
    for (const input of fieldset.querySelectorAll<HTMLInputElement>('input[type="radio"]')) {
      if (input.name === name && input.form === event.target) {
        input.checked = input.value === resetValue
      }
    }
  })

  // Options can arrive or change form ownership without changing the group name.
  // Refresh after every render, cleaning up the previous owning-form subscriptions.
  useEffect(() => {
    const fieldset = fieldsetRef.current
    const inputs = () =>
      Array.from(fieldset?.querySelectorAll<HTMLInputElement>('input[type="radio"]') ?? []).filter(
        (input) => input.name === name,
      )
    const forms = new Set(
      inputs()
        .map((input) => input.form)
        .filter((form) => form !== null),
    )
    function reset(event: Event) {
      // Native reset runs after the event. Respect cancellation and restore React's ownership
      // afterwards, even when the selected value did not change (and React would bail out).
      queueMicrotask(() => {
        restoreAfterReset(event)
      })
    }
    for (const form of forms) form.addEventListener('reset', reset)
    return () => {
      for (const form of forms) form.removeEventListener('reset', reset)
    }
  })

  function select(next: string) {
    if (!isControlled) {
      setUncontrolledValue(next)
    }
    onChange?.(next)
  }

  return (
    <div className={className} data-component="radios">
      <RadiosContext.Provider
        value={{
          describedBy: descriptions,
          error: Boolean(error),
          inline,
          name,
          onBlur,
          select,
          selectedValue,
        }}
      >
        <fieldset
          aria-describedby={descriptions}
          aria-invalid={error ? true : undefined}
          className="form-group"
          data-component={`form-group${inline ? '--inline' : ''}`}
          ref={fieldsetRef}
        >
          <legend
            className={clsx(
              'form-group__legend',
              Heading !== 'span' && 'form-group__legend--with-heading',
            )}
          >
            {Heading === 'span' ? legend : <Heading>{legend}</Heading>}
          </legend>
          {errorId && (
            <p className="form-group__error-message" id={errorId}>
              <span className="visually-hidden">Error:</span> {error}
            </p>
          )}
          {hintId && (
            <p className="form-group__hint" id={hintId}>
              {hint}
            </p>
          )}
          {children}
        </fieldset>
      </RadiosContext.Provider>
    </div>
  )
}

export type RadioProps = Omit<
  ComponentPropsWithoutRef<'input'>,
  'aria-invalid' | 'checked' | 'defaultChecked' | 'name' | 'onChange' | 'type' | 'value'
> & {
  /**
   * Content revealed underneath this option while it is selected, such as a single related
   * question. Keep it simple: do not reveal more than one question, and do not nest conditional
   * radios. Ignored when the group is inline. The content stays mounted while hidden, so values
   * entered are kept if the user switches option and back. Inactive controls are disabled,
   * so they are excluded from native validation and FormData while hidden.
   */
  conditional?: ReactNode
  hint?: string
  error?: boolean
  errorMessage?: string
  label: ReactNode
  value: string
}

/** One option in `Radios`. */
export function Radio({
  'aria-describedby': describedBy,
  className,
  conditional,
  error: optionError,
  errorMessage,
  hint,
  id,
  label,
  onBlur: optionOnBlur,
  value,
  ...inputProps
}: RadioProps) {
  const {
    describedBy: groupDescriptions,
    error: groupError,
    inline,
    name,
    onBlur,
    select,
    selectedValue,
  } = useRadiosContext()
  const error = groupError || optionError
  const inputId = id ?? `${name}_${value}`
  const hintId = hint ? `${inputId}-hint` : undefined
  const errorId = optionError && errorMessage ? `${inputId}-error` : undefined
  const descriptions =
    [describedBy, groupDescriptions, hintId, errorId].filter(Boolean).join(' ') || undefined
  const checked = selectedValue === value
  const hasConditional = conditional !== undefined && conditional !== null && !inline
  const conditionalId = `conditional-${inputId}`

  return (
    <>
      {errorId && (
        <p className="radio__error-message" id={errorId}>
          <span className="visually-hidden">Error:</span> {errorMessage}
        </p>
      )}
      <div
        className={clsx('radio', inline && 'radio--inline', error && 'radio--error')}
        data-component={`radio${inline ? '--inline' : ''}`}
      >
        <input
          {...inputProps}
          aria-controls={hasConditional ? conditionalId : undefined}
          aria-describedby={descriptions}
          checked={checked}
          className={clsx('radio__input', className)}
          id={inputId}
          name={name}
          onBlur={(event) => {
            optionOnBlur?.(event)
            onBlur?.(event)
          }}
          onChange={() => select(value)}
          type="radio"
          value={value}
        />
        <label className="radio__label" htmlFor={inputId}>
          {label}
        </label>
        {hintId && (
          <span className="radio__hint" id={hintId}>
            {hint}
          </span>
        )}
      </div>
      {hasConditional && (
        <fieldset
          className={styles.conditional}
          data-component="radios-conditional"
          disabled={!checked}
          hidden={!checked}
          id={conditionalId}
        >
          {conditional}
        </fieldset>
      )}
    </>
  )
}
