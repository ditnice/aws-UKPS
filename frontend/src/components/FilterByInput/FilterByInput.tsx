'use client'

import clsx from 'clsx'
import { useState, useSyncExternalStore } from 'react'

import ChevronDownIcon from '@nice-digital/icons/lib/ChevronDown'
import '@nice-digital/nds-filters/scss/filter-input.scss'

import { Button } from '@/components/Button/Button'
import { Input, type InputProps } from '@/components/Input/Input'

import styles from './FilterByInput.module.scss'

const subscribe = () => () => undefined

export type FilterByInputProps = {
  buttonLabel?: string
  className?: string
  collapseByDefault?: boolean
  /** Set automatically when rendered inside a FilterPanel. */
  headingLevel?: 3 | 4 | 5 | 6
  /** Shown beneath the label in the heading box, and used to describe the input. */
  hint?: string
  inputProps?: Omit<InputProps, 'label' | 'name' | 'type'>
  label: string
  name: string
  type?: InputProps['type']
}

/**
 * Mirrors the NDS FilterByInput markup, with support for hint text.
 */
export function FilterByInput({
  buttonLabel = 'Filter',
  className,
  collapseByDefault = false,
  headingLevel = 3,
  hint,
  inputProps,
  label,
  name,
  type = 'text',
}: FilterByInputProps) {
  const [isExpanded, setIsExpanded] = useState(!collapseByDefault)
  const canUseDOM = useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  )

  const HeadingLevel = `h${headingLevel}` as const
  const controlsId = `inputFilter-${name}`
  const hintId = hint ? `${controlsId}-hint` : undefined
  const ariaDescribedBy =
    [inputProps?.['aria-describedby'], hintId].filter(Boolean).join(' ') || undefined

  return (
    <div className={clsx('inputFilterBox', className)} data-component="input-filter">
      <div className={styles.header}>
        <HeadingLevel className="inputFilterBox__heading">
          {canUseDOM ? (
            <button
              type="button"
              aria-expanded={isExpanded}
              aria-controls={controlsId}
              onClick={() => setIsExpanded((current) => !current)}
            >
              <ChevronDownIcon
                className={clsx(
                  'filter-group__heading-icon',
                  isExpanded && 'filter-group__heading-icon--expanded',
                )}
              />
              {label}
            </button>
          ) : (
            label
          )}
        </HeadingLevel>
        {hint && (
          <p className={styles.hint} id={hintId}>
            {hint}
          </p>
        )}
      </div>
      <div id={controlsId} aria-hidden={!isExpanded} className="inputFilterBox__controls">
        <Input
          {...inputProps}
          aria-describedby={ariaDescribedBy}
          label={label}
          name={name}
          type={type}
        />
        <Button buttonType="submit" className="ml--0 mb--0">
          {buttonLabel}
        </Button>
      </div>
    </div>
  )
}

// FilterPanel only passes headingLevel to children with this display name.
FilterByInput.displayName = 'FilterByInput'
