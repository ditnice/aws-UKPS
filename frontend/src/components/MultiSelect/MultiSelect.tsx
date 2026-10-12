'use client'

import clsx from 'clsx'
import { useEffect, useId, useImperativeHandle, useRef, useState } from 'react'

import Check from '@nice-digital/icons/lib/Check'
import ChevronDown from '@nice-digital/icons/lib/ChevronDown'
import Remove from '@nice-digital/icons/lib/Remove'

import { inputWidthStyles } from '@/components/Input/Input'
import type { InputWidth } from '@/components/Input/Input'

import styles from './MultiSelect.module.scss'

import type { AriaAttributes, CSSProperties, KeyboardEvent, Ref } from 'react'

export type MultiSelectOption = {
  value: string
  label: string
  disabled?: boolean
}

export type MultiSelectProps = Pick<AriaAttributes, 'aria-describedby' | 'aria-invalid'> & {
  name: string
  label: string
  /** Values must be unique. Include selected options even when they are disabled. */
  options: readonly MultiSelectOption[]
  value: readonly string[]
  onChange: (values: string[]) => void
  /** Called when focus leaves the whole component, not when moving to a remove button. */
  onBlur?: () => void
  id?: string
  hint?: string
  placeholder?: string
  /** Omit for unlimited selections. Zero prevents additions but still permits removal. */
  maxSelections?: number
  error?: boolean
  errorMessage?: string
  disabled?: boolean
  inputRef?: Ref<HTMLInputElement>
  width?: InputWidth
  className?: string
  style?: CSSProperties
}

export function MultiSelect({
  'aria-describedby': describedBy,
  'aria-invalid': ariaInvalid,
  name,
  label,
  options,
  value,
  onChange,
  onBlur,
  id,
  hint,
  placeholder = 'Choose or search options',
  maxSelections,
  error,
  errorMessage,
  disabled = false,
  inputRef,
  width,
  className,
  style,
}: MultiSelectProps) {
  const uniqueId = useId()
  const inputId = id ?? `${name}-${uniqueId}`
  const listId = `${inputId}-options`
  const rootRef = useRef<HTMLDivElement>(null)
  const internalInputRef = useRef<HTMLInputElement>(null)
  const removeRefs = useRef(new Map<string, HTMLButtonElement>())
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [activeValue, setActiveValue] = useState<string | null>(null)
  const [announcement, setAnnouncement] = useState('')
  const selectedValues = [...new Set(value)]
  const limitReached = maxSelections !== undefined && selectedValues.length >= maxSelections
  const expanded = open && !disabled
  const filteredOptions = options.filter((option) =>
    option.label.toLowerCase().includes(query.trim().toLowerCase()),
  )
  const activeOption = filteredOptions.find((option) => option.value === activeValue)
  const optionId = (option: MultiSelectOption) => `${listId}-${options.indexOf(option)}`
  const activeId = activeOption ? optionId(activeOption) : undefined
  const limitMessage = limitReached
    ? 'Selection limit reached. Remove a selected option before choosing another.'
    : ''
  const description = [
    describedBy,
    hint && `${inputId}-hint`,
    error && errorMessage && `${inputId}-error`,
    `${inputId}-instructions`,
    maxSelections !== undefined && `${inputId}-count`,
  ]
    .filter(Boolean)
    .join(' ')

  useImperativeHandle(inputRef, () => internalInputRef.current!, [])

  useEffect(() => {
    if (expanded && activeId) {
      const option = document.getElementById(activeId)
      const panel = option?.parentElement
      if (!option || !panel) return
      const optionRect = option.getBoundingClientRect()
      const panelRect = panel.getBoundingClientRect()
      const top = panelRect.top + panel.clientTop
      const bottom = top + panel.clientHeight
      // Scroll only the panel: scrollIntoView can also move the page and obscure the input.
      if (optionRect.top < top || optionRect.height > panel.clientHeight) {
        panel.scrollTop += optionRect.top - top
      } else if (optionRect.bottom > bottom) {
        panel.scrollTop += optionRect.bottom - bottom
      }
    }
  }, [expanded, activeId])

  useEffect(() => {
    if (!expanded) return
    function closeOutside(event: PointerEvent) {
      if (event.target instanceof Node && !rootRef.current?.contains(event.target)) {
        setOpen(false)
        setActiveValue(null)
      }
    }
    document.addEventListener('pointerdown', closeOutside)
    return () => document.removeEventListener('pointerdown', closeOutside)
  }, [expanded])

  function toggleOption(option: MultiSelectOption) {
    if (disabled || option.disabled) return
    const selected = selectedValues.includes(option.value)
    if (!selected && limitReached) {
      setAnnouncement(`${option.label} not selected.`)
      return
    }
    onChange(
      selected
        ? selectedValues.filter((entry) => entry !== option.value)
        : [...selectedValues, option.value],
    )
    setAnnouncement(`${option.label} ${selected ? 'removed' : 'selected'}.`)
    setQuery('')
    setActiveValue(option.value)
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.nativeEvent.isComposing) return
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault()
      const navigable = filteredOptions.filter((option) => !option.disabled)
      const index = expanded ? navigable.findIndex((option) => option.value === activeValue) : -1
      const nextIndex =
        event.key === 'ArrowDown'
          ? Math.min(index + 1, navigable.length - 1)
          : index < 0
            ? navigable.length - 1
            : Math.max(0, index - 1)
      setActiveValue(navigable[nextIndex]?.value ?? null)
      setOpen(true)
    } else if (event.key === 'Enter' && expanded) {
      event.preventDefault()
      if (activeOption) toggleOption(activeOption)
    } else if (event.key === 'Escape' && expanded) {
      event.preventDefault()
      event.stopPropagation()
      setOpen(false)
      setActiveValue(null)
    } else if (event.key === 'Tab') {
      setOpen(false)
      setActiveValue(null)
    }
  }

  function removeSelection(entry: string) {
    const index = selectedValues.indexOf(entry)
    const next = selectedValues[index + 1] ?? selectedValues[index - 1]
    const target = next ? removeRefs.current.get(next) : internalInputRef.current
    target?.focus()
    onChange(selectedValues.filter((selected) => selected !== entry))
    setAnnouncement(`${options.find((option) => option.value === entry)?.label ?? entry} removed.`)
  }

  return (
    <div
      className={clsx(styles.multiSelect, className)}
      data-component="multi-select"
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) {
          setOpen(false)
          setActiveValue(null)
          onBlur?.()
        }
      }}
      ref={rootRef}
      style={style}
    >
      <label className={styles.label} htmlFor={inputId} id={`${inputId}-label`}>
        {label}
      </label>
      {hint && <p id={`${inputId}-hint`}>{hint}</p>}
      {error && errorMessage && (
        <p className={styles.error} id={`${inputId}-error`}>
          <span className={styles.visuallyHidden}>Error:</span> {errorMessage}
        </p>
      )}
      <p className={styles.visuallyHidden} id={`${inputId}-instructions`}>
        Type to filter options. Use the up and down arrow keys to move through options, Enter to
        select or deselect, and Escape to close. Selected options have remove buttons below.
      </p>
      <div className={styles.control} style={width ? inputWidthStyles[width] : undefined}>
        <div className={styles.inputRow}>
          <input
            aria-activedescendant={expanded ? activeId : undefined}
            aria-autocomplete="list"
            aria-controls={listId}
            aria-describedby={description}
            aria-expanded={expanded}
            aria-haspopup="listbox"
            aria-invalid={error ? true : ariaInvalid}
            autoComplete="off"
            className={clsx(styles.field, error && styles.fieldError)}
            disabled={disabled}
            id={inputId}
            onChange={(event) => {
              setQuery(event.target.value)
              setActiveValue(null)
              setAnnouncement('')
              setOpen(true)
            }}
            onClick={() => setOpen(true)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            ref={internalInputRef}
            role="combobox"
            type="text"
            value={query}
          />
          <button
            aria-controls={listId}
            aria-expanded={expanded}
            aria-label={`${expanded ? 'Hide' : 'Show'} options for ${label}`}
            className={styles.toggle}
            disabled={disabled}
            onClick={() => {
              setOpen(!expanded)
              setActiveValue(null)
              internalInputRef.current?.focus()
            }}
            onMouseDown={(event) => event.preventDefault()}
            type="button"
          >
            <ChevronDown aria-hidden="true" className={clsx(expanded && styles.expandedIcon)} />
          </button>
        </div>
        <ul
          aria-labelledby={`${inputId}-label`}
          aria-multiselectable="true"
          className={styles.options}
          hidden={!expanded}
          id={listId}
          role="listbox"
        >
          {filteredOptions.map((option) => {
            const selected = selectedValues.includes(option.value)
            const unavailable = option.disabled || (!selected && limitReached)
            return (
              <li
                aria-disabled={unavailable || undefined}
                aria-selected={selected}
                className={clsx(styles.option, activeOption === option && styles.active)}
                id={optionId(option)}
                key={option.value}
                onClick={() => {
                  internalInputRef.current?.focus()
                  toggleOption(option)
                }}
                onMouseDown={(event) => event.preventDefault()}
                role="option"
              >
                <span aria-hidden="true" className={styles.selectionIndicator}>
                  {selected && <Check aria-hidden="true" />}
                </span>
                {option.label}
              </li>
            )
          })}
        </ul>
        {expanded && filteredOptions.length === 0 && (
          <p className={styles.empty}>No options found.</p>
        )}
      </div>
      {maxSelections !== undefined && (
        <p id={`${inputId}-count`}>
          {selectedValues.length} of {maxSelections} selected. {limitMessage}
        </p>
      )}
      {selectedValues.length > 0 && (
        <ul aria-label={`Selected options for ${label}`} className={styles.selections}>
          {selectedValues.map((entry) => {
            const optionLabel = options.find((option) => option.value === entry)?.label ?? entry
            return (
              <li className={styles.selection} key={entry}>
                <span>{optionLabel}</span>
                <button
                  aria-label={`Remove ${optionLabel}`}
                  className={styles.remove}
                  disabled={disabled}
                  onClick={() => removeSelection(entry)}
                  ref={(element) => {
                    if (element) removeRefs.current.set(entry, element)
                    else removeRefs.current.delete(entry)
                  }}
                  type="button"
                >
                  <Remove aria-hidden="true" />
                </button>
              </li>
            )
          })}
        </ul>
      )}
      {selectedValues.map((entry) => (
        <input disabled={disabled} key={entry} name={name} type="hidden" value={entry} />
      ))}
      <div aria-atomic="true" aria-live="polite" className={styles.visuallyHidden} role="status">
        {announcement} {expanded && `${filteredOptions.length} options available.`} {limitMessage}
      </div>
    </div>
  )
}
