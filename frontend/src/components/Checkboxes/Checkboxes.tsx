'use client'

import clsx from 'clsx'
import {
  createContext,
  useContext,
  useEffect,
  useEffectEvent,
  useId,
  useRef,
  useState,
} from 'react'

import '@nice-digital/nds-checkbox/scss/checkbox.scss'

import { composeDescriptions, FormGroup, useFormGroup } from '@/components/FormGroup/FormGroup'
import type { FormGroupProps } from '@/components/FormGroup/FormGroup'

import type { ComponentPropsWithRef, FocusEventHandler, ReactNode } from 'react'

type Selection = {
  name: string
  values: string[]
  toggle: (value: string, checked: boolean) => void
  onBlur?: FocusEventHandler<HTMLInputElement>
}
const CheckboxSelection = createContext<Selection | null>(null)
const useCheckboxSelection = () => useContext(CheckboxSelection)

export type CheckboxesProps = Omit<
  FormGroupProps,
  'onChange' | 'onBlur' | 'defaultValue' | 'ref' | 'name' | 'legend'
> & {
  legend: string
  name: string
  value?: string[]
  defaultValue?: string[]
  onChange?: (values: string[]) => void
  onBlur?: FocusEventHandler<HTMLInputElement>
}

/** Multiple selections submit repeated name/value pairs (read with FormData.getAll). */
export function Checkboxes({
  name,
  value,
  defaultValue,
  onChange,
  onBlur,
  children,
  ...groupProps
}: CheckboxesProps) {
  const [uncontrolledValue, setUncontrolledValue] = useState(() => [...new Set(defaultValue ?? [])])
  const controlled = value !== undefined
  const values = controlled ? value : uncontrolledValue
  const fieldsetRef = useRef<HTMLFieldSetElement>(null)
  const restore = useEffectEvent((event: Event) => {
    const fieldset = fieldsetRef.current
    if (event.defaultPrevented || !fieldset?.isConnected) return
    const next = [...new Set(controlled ? value : (defaultValue ?? []))]
    if (!controlled) setUncontrolledValue(next)
    for (const input of fieldset.querySelectorAll<HTMLInputElement>('input[type="checkbox"]')) {
      if (input.name === name && input.form === event.target)
        input.checked = next.includes(input.value)
    }
  })
  // Refresh ownership subscriptions for late options and changed native form attributes.
  useEffect(() => {
    const forms = new Set(
      Array.from(
        fieldsetRef.current?.querySelectorAll<HTMLInputElement>('input[type="checkbox"]') ?? [],
      )
        .filter((input) => input.name === name)
        .map((input) => input.form)
        .filter((form) => form !== null),
    )
    const reset = (event: Event) => queueMicrotask(() => restore(event))
    for (const form of forms) form.addEventListener('reset', reset)
    return () => {
      for (const form of forms) form.removeEventListener('reset', reset)
    }
  })
  function toggle(option: string, checked: boolean) {
    const next = checked
      ? [...new Set([...values, option])]
      : [...new Set(values.filter((item) => item !== option))]
    if (!controlled) setUncontrolledValue(next)
    onChange?.(next)
  }
  return (
    <CheckboxSelection.Provider value={{ name, values, toggle, onBlur }}>
      <FormGroup {...groupProps} identifier={groupProps.identifier ?? name} ref={fieldsetRef}>
        {children}
      </FormGroup>
    </CheckboxSelection.Provider>
  )
}

export type CheckboxProps = Omit<ComponentPropsWithRef<'input'>, 'type' | 'value'> & {
  label: ReactNode
  /** Required option value; the enclosing Checkboxes owns the name and selection. */
  value: string
  hint?: ReactNode
  error?: boolean
  errorMessage?: string
}

/** A native standalone checkbox, or an option controlled by its enclosing Checkboxes. */
export function Checkbox({
  'aria-describedby': describedBy,
  'aria-invalid': invalid,
  label,
  value,
  name,
  id,
  hint,
  error,
  errorMessage,
  className,
  onChange,
  onBlur,
  checked,
  defaultChecked,
  ...rest
}: CheckboxProps) {
  const selection = useCheckboxSelection()
  const group = useFormGroup()
  const generatedId = useId()
  const inputId = id ?? (selection ? `${selection.name}_${value}` : generatedId)
  const hintId = hint ? `${inputId}-hint` : undefined
  const errorId = error && errorMessage ? `${inputId}-error` : undefined
  const isInvalid = error || group.error
  return (
    <>
      {errorId && (
        <p className="checkbox__error-message" id={errorId}>
          <span className="visually-hidden">Error:</span> {errorMessage}
        </p>
      )}
      <div
        className={clsx(
          'checkbox',
          group.inline && 'checkbox--inline',
          isInvalid && 'checkbox--error',
        )}
        data-component={`checkbox${group.inline ? '--inline' : ''}`}
      >
        <input
          {...rest}
          aria-describedby={composeDescriptions(describedBy, group.describedBy, hintId, errorId)}
          aria-invalid={isInvalid ? true : invalid}
          checked={selection ? selection.values.includes(value) : checked}
          defaultChecked={selection ? undefined : defaultChecked}
          className={clsx('checkbox__input', className)}
          id={inputId}
          name={selection?.name ?? name}
          onBlur={(event) => {
            onBlur?.(event)
            selection?.onBlur?.(event)
          }}
          onChange={(event) => {
            selection?.toggle(value, event.target.checked)
            onChange?.(event)
          }}
          type="checkbox"
          value={value}
        />
        <label className="checkbox__label" htmlFor={inputId}>
          {label}
        </label>
        {hintId && (
          <div className="checkbox__hint" id={hintId}>
            {hint}
          </div>
        )}
      </div>
    </>
  )
}
