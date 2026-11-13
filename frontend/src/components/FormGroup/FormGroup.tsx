'use client'

import clsx from 'clsx'
import { createContext, useContext, useId } from 'react'

import '@nice-digital/nds-form-group/scss/form-group.scss'

import styles from './FormGroup.module.scss'

import type { ComponentPropsWithRef, ReactNode } from 'react'

type GroupMetadata = { describedBy?: string; error: boolean; inline: boolean }
const FormGroupContext = createContext<GroupMetadata>({ error: false, inline: false })
export const useFormGroup = () => useContext(FormGroupContext)

/** Compose ID references without describing the same text twice. */
export function composeDescriptions(...values: (string | undefined)[]) {
  return (
    [...new Set(values.flatMap((value) => value?.split(/\s+/).filter(Boolean) ?? []))].join(' ') ||
    undefined
  )
}

export type FormGroupProps = ComponentPropsWithRef<'fieldset'> & {
  legend?: ReactNode
  hint?: ReactNode
  error?: string
  headingLevel?: 1 | 2 | 3 | 4 | 5 | 6
  inline?: boolean
  /** Stable prefix for hint/error IDs; otherwise React generates a hydration-safe ID. */
  identifier?: string
}

/** Presentational grouping only: children opt into metadata, and are never cloned. */
export function FormGroup({
  'aria-describedby': describedBy,
  children,
  className,
  legend,
  hint,
  error,
  headingLevel,
  inline = false,
  identifier,
  ...rest
}: FormGroupProps) {
  const generatedId = useId()
  const prefix = identifier ?? generatedId
  const hintId = hint ? `${prefix}-hint` : undefined
  const errorId = error ? `${prefix}-error` : undefined
  const descriptions = composeDescriptions(describedBy, hintId, errorId)
  const Heading = headingLevel ? (`h${headingLevel}` as const) : 'span'
  return (
    <FormGroupContext.Provider value={{ describedBy: descriptions, error: Boolean(error), inline }}>
      <fieldset
        {...rest}
        aria-describedby={descriptions}
        className={clsx('form-group', className)}
        data-component={`form-group${inline ? '--inline' : ''}`}
      >
        {legend != null && (
          <legend
            className={clsx(
              'form-group__legend',
              Heading !== 'span' && 'form-group__legend--with-heading',
            )}
          >
            {Heading === 'span' ? (
              legend
            ) : (
              <Heading className={styles.legendHeading}>{legend}</Heading>
            )}
          </legend>
        )}
        {hintId && (
          <p className="form-group__hint" id={hintId}>
            {hint}
          </p>
        )}
        {errorId && (
          <p className="form-group__error-message" id={errorId}>
            <span className="visually-hidden">Error:</span> {error}
          </p>
        )}
        {children}
      </fieldset>
    </FormGroupContext.Provider>
  )
}
