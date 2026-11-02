'use client'

import clsx from 'clsx'
import { createContext, useContext, useState } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio as NdsRadio } from '@nice-digital/nds-radio'

import styles from './Radios.module.scss'

import type { ComponentPropsWithoutRef, FocusEventHandler, ReactNode } from 'react'

type RadiosContextValue = {
  error: boolean
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
 * A group of radios, wrapping the NDS form group and radio. Individual options can
 * conditionally reveal a related question when selected - see `Radio`'s `conditional` prop and
 * https://design-system.service.gov.uk/components/radios/#conditionally-revealing-a-related-question
 */
export function Radios({
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

  function select(next: string) {
    if (!isControlled) {
      setUncontrolledValue(next)
    }
    onChange?.(next)
  }

  return (
    <div className={className} data-component="radios">
      <RadiosContext.Provider
        value={{ error: Boolean(error), inline, name, onBlur, select, selectedValue }}
      >
        <FormGroup
          groupError={error}
          headingLevel={headingLevel}
          hint={hint}
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
  'checked' | 'defaultChecked' | 'name' | 'onChange' | 'type' | 'value'
> & {
  /**
   * Content revealed underneath this option while it is selected, such as a single related
   * question. Keep it simple: do not reveal more than one question, and do not nest conditional
   * radios. Ignored when the group is inline. The content stays mounted while hidden, so values
   * entered are kept if the user switches option and back.
   */
  conditional?: ReactNode
  hint?: string
  label: ReactNode
  value: string
}

/** One option in `Radios`. */
export function Radio({ conditional, hint, label, value, ...rest }: RadioProps) {
  // NDS FormGroup clones its children with `inline` and `name`; the group context is the source of
  // truth for both, so drop the cloned copies rather than spreading them onto the input.
  const {
    inline: _inline,
    name: _name,
    ...inputProps
  } = rest as typeof rest & { inline?: boolean; name?: string }
  const { error, inline, name, onBlur, select, selectedValue } = useRadiosContext()
  const checked = selectedValue === value
  const hasConditional = conditional !== undefined && conditional !== null && !inline
  const conditionalId = `conditional-${name}_${value}`

  return (
    <>
      <NdsRadio
        onBlur={onBlur}
        {...inputProps}
        aria-controls={hasConditional ? conditionalId : undefined}
        aria-expanded={hasConditional ? checked : undefined}
        checked={checked}
        error={error}
        hint={hint}
        inline={inline}
        label={label}
        name={name}
        onChange={() => select(value)}
        value={value}
      />
      {hasConditional && (
        <div
          className={clsx(styles.conditional, !checked && styles.conditionalHidden)}
          data-component="radios-conditional"
          id={conditionalId}
        >
          {conditional}
        </div>
      )}
    </>
  )
}
