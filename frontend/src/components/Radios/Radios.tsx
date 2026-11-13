'use client'

import clsx from 'clsx'
import { createContext, useContext, useId, useState } from 'react'

import '@nice-digital/nds-radio/scss/radio.scss'

import {
  composeDescriptions,
  firstOptionValue,
  FormGroup,
  useFormGroup,
} from '@/components/FormGroup/FormGroup'
import type { FormGroupProps } from '@/components/FormGroup/FormGroup'

import styles from './Radios.module.scss'

import type { ComponentPropsWithoutRef, FocusEventHandler, ReactNode } from 'react'

type RadiosContextValue = {
  /** Value of the first option, the only one styled as invalid for a group error. */
  firstValue?: string
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
  /** `Radio` options, rendered directly or in Fragments so the first can show a group error. */
  children: ReactNode
  className?: string
  /** Initial selection when uncontrolled. */
  defaultValue?: string
  /** Error message shown above the options; also styles the first option as invalid. */
  error?: string
  /** Renders the legend inside a heading of this level, e.g. on a single-question page. */
  headingLevel?: FormGroupProps['headingLevel']
  hint?: ReactNode
  /** Stable prefix for the group's hint and error IDs; otherwise React generates one. */
  identifier?: string
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
  identifier,
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
  const firstValue = firstOptionValue(children, Radio)

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
          firstValue,
          name,
          onBlur,
          select,
          selectedValue,
        }}
      >
        <FormGroup
          aria-describedby={describedBy}
          error={error}
          headingLevel={headingLevel}
          hint={hint}
          identifier={identifier}
          inline={inline}
          legend={legend}
        >
          {children}
        </FormGroup>
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
  const { firstValue, name, onBlur, select, selectedValue } = useRadiosContext()
  const { describedBy: groupDescriptions, error: groupError, inline } = useFormGroup()
  const error = (groupError && value === firstValue) || optionError
  const generatedId = useId()
  const inputId = id ?? generatedId
  const hintId = hint ? `${inputId}-hint` : undefined
  const errorId = optionError && errorMessage ? `${inputId}-error` : undefined
  const descriptions = composeDescriptions(describedBy, groupDescriptions, hintId, errorId)
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
        <label className={clsx('radio__label', !hint && styles.labelPlain)} htmlFor={inputId}>
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
