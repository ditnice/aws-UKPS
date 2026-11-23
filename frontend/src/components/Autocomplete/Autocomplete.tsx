'use client'

import clsx from 'clsx'
import { useEffect, useRef, useState } from 'react'

import '@nice-digital/nds-input/scss/input.scss'

import { inputWidthStyles } from '@/components/Input/Input'
import type { InputWidth, LabelSize } from '@/components/Input/Input'

import styles from './Autocomplete.module.scss'

import type { KeyboardEvent, ReactNode, Ref } from 'react'

/** Fetches the suggestions for a query. Abort the request when `signal` aborts. */
export type GetSuggestions = (query: string, signal: AbortSignal) => Promise<string[]>

export type AutocompleteProps = {
  className?: string
  /** Milliseconds to wait after the last keystroke before fetching suggestions. */
  debounceMs?: number
  error?: boolean
  errorMessage?: string
  getSuggestions: GetSuggestions
  hint?: string
  id?: string
  inputRef?: Ref<HTMLInputElement>
  label: string | null
  /** As for `Input`: the standard bold label, or bold lead text on a page of several questions. */
  labelSize?: LabelSize
  /** The fewest characters, ignoring surrounding spaces, that fetch suggestions. */
  minQueryLength?: number
  name: string
  onBlur?: () => void
  onChange: (value: string) => void
  value: string
  /** As for `Input`: applied to the field, which the suggestions match in width. */
  width?: InputWidth
}

const usageHint =
  'When suggestions are available, use up and down arrows to review and enter to select. ' +
  'Touch device users, explore by touch or with swipe gestures.'

/**
 * The status read out by screen readers. `alternate` adds a non-breaking space on alternate
 * updates, so a result with the same count as the last one still changes the text and is read
 * out again, as in GOV.UK's accessible-autocomplete.
 */
const statusText = (count: number, alternate: boolean) =>
  (count === 0
    ? 'No suggestions available.'
    : `${count} ${count === 1 ? 'suggestion' : 'suggestions'} available.`) +
  (alternate ? '\u00a0' : '')

/** Emphasises the first part of the suggestion that matches the query, ignoring case. */
const highlightMatch = (suggestion: string, query: string): ReactNode => {
  const start = suggestion.toLowerCase().indexOf(query.toLowerCase())
  if (!query || start === -1) {
    return suggestion
  }
  const end = start + query.length
  return (
    <>
      {suggestion.slice(0, start)}
      <strong className={styles.match}>{suggestion.slice(start, end)}</strong>
      {suggestion.slice(end)}
    </>
  )
}

const isAbortError = (error: unknown) =>
  error instanceof DOMException && error.name === 'AbortError'

/**
 * A text input that suggests values as the user types, following the WAI-ARIA 1.2 combobox
 * pattern with list autocomplete. Suggestions are optional: any text the user enters is kept.
 * https://www.w3.org/WAI/ARIA/apg/patterns/combobox/examples/combobox-autocomplete-list/
 */
export function Autocomplete({
  className,
  debounceMs = 250,
  error,
  errorMessage,
  getSuggestions,
  hint,
  id,
  inputRef,
  label,
  labelSize = 'default',
  minQueryLength = 1,
  name,
  onBlur,
  onChange,
  value,
  width,
}: AutocompleteProps) {
  const inputId = id ?? name
  const listboxId = `${inputId}-listbox`
  const optionId = (index: number) => `${inputId}-option-${index}`
  const hintId = hint ? `${inputId}-hint` : undefined
  const errorId = error && errorMessage ? `${inputId}-error` : undefined
  const usageHintId = `${inputId}-usage`
  const describedBy = [hintId, errorId, usageHintId].filter(Boolean).join(' ')

  const [suggestions, setSuggestions] = useState<string[]>([])
  const [query, setQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)
  const [activeIndex, setActiveIndex] = useState(-1)
  const [status, setStatus] = useState('')

  const timer = useRef<ReturnType<typeof setTimeout>>(undefined)
  const request = useRef<AbortController>(undefined)
  const listbox = useRef<HTMLUListElement>(null)
  const alternateStatus = useRef(false)

  const cancelPending = () => {
    clearTimeout(timer.current)
    request.current?.abort()
  }

  useEffect(() => cancelPending, [])

  // Keep the active suggestion in view as the user arrows through a scrolling list.
  useEffect(() => {
    if (activeIndex >= 0) {
      listbox.current?.children[activeIndex]?.scrollIntoView?.({ block: 'nearest' })
    }
  }, [activeIndex])

  const close = () => {
    setIsOpen(false)
    setActiveIndex(-1)
  }

  const fetchSuggestions = (text: string) => {
    cancelPending()
    // Results and the active selection belong to the previous query, not the new input.
    setSuggestions([])
    setStatus('')
    close()
    const trimmed = text.trim()

    if (trimmed.length < minQueryLength) {
      return
    }

    timer.current = setTimeout(() => {
      const controller = new AbortController()
      request.current = controller

      getSuggestions(trimmed, controller.signal).then(
        (results) => {
          if (controller.signal.aborted) {
            return
          }
          alternateStatus.current = !alternateStatus.current
          setSuggestions(results)
          setQuery(trimmed)
          setActiveIndex(-1)
          setIsOpen(results.length > 0)
          setStatus(statusText(results.length, alternateStatus.current))
        },
        (reason: unknown) => {
          // Suggestions are optional, so a failure leaves a plain text input.
          if (!isAbortError(reason) && !controller.signal.aborted) {
            setSuggestions([])
            setStatus('')
            close()
          }
        },
      )
    }, debounceMs)
  }

  const select = (index: number) => {
    cancelPending()
    onChange(suggestions[index])
    // The suggestions were for the text before the choice, so they are not offered again.
    setSuggestions([])
    setStatus('')
    close()
  }

  // Leaving the field also drops a fetch still waiting or running, so its results can't open
  // the list under a field that no longer has focus.
  const leave = () => {
    cancelPending()
    close()
  }

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    const count = suggestions.length

    switch (event.key) {
      case 'ArrowDown': {
        if (count === 0) {
          return
        }
        event.preventDefault()
        if (!isOpen) {
          setIsOpen(true)
          // Alt+Down opens the list without choosing a suggestion.
          setActiveIndex(event.altKey ? -1 : 0)
          return
        }
        // After the last suggestion, return to the text the user typed.
        setActiveIndex((index) => (index + 1 >= count ? -1 : index + 1))
        return
      }
      case 'ArrowUp': {
        if (count === 0) {
          return
        }
        event.preventDefault()
        if (!isOpen) {
          setIsOpen(true)
          setActiveIndex(count - 1)
          return
        }
        setActiveIndex((index) => (index < 0 ? count - 1 : index - 1))
        return
      }
      case 'Enter': {
        // Without an active suggestion, Enter submits the form as usual.
        if (isOpen && activeIndex >= 0) {
          event.preventDefault()
          select(activeIndex)
        }
        return
      }
      case 'Escape': {
        if (isOpen) {
          event.preventDefault()
        }
        // Dismiss pending results too, even if the list has not opened yet.
        cancelPending()
        close()
        setStatus('')
        return
      }
      case 'Tab': {
        leave()
        return
      }
    }
  }

  const expanded = isOpen && suggestions.length > 0

  return (
    <div
      className={clsx('input', error && 'input--error', className)}
      data-component="autocomplete"
    >
      {label && (
        <label
          className={clsx('input__label', labelSize === 'lead' && styles.leadLabel)}
          htmlFor={inputId}
        >
          {label}
        </label>
      )}
      {hint && (
        <p className="input__hint" id={hintId}>
          {hint}
        </p>
      )}
      {errorId && (
        <p className="input__error" id={errorId}>
          <span className="visually-hidden">Error:</span> {errorMessage}
        </p>
      )}
      <div className={styles.wrapper} style={width ? inputWidthStyles[width] : undefined}>
        <input
          aria-activedescendant={expanded && activeIndex >= 0 ? optionId(activeIndex) : undefined}
          aria-autocomplete="list"
          aria-controls={listboxId}
          aria-describedby={describedBy}
          aria-expanded={expanded}
          aria-invalid={error ? true : undefined}
          autoCapitalize="none"
          autoComplete="off"
          className={clsx('input__input', styles.input)}
          id={inputId}
          name={name}
          onBlur={() => {
            leave()
            onBlur?.()
          }}
          onChange={(event) => {
            onChange(event.target.value)
            fetchSuggestions(event.target.value)
          }}
          onKeyDown={handleKeyDown}
          ref={inputRef}
          role="combobox"
          spellCheck={false}
          type="text"
          value={value}
        />
        <ul
          className={styles.listbox}
          hidden={!expanded}
          id={listboxId}
          // Keep focus on the input, so choosing or scrolling with a pointer doesn't close the list.
          onMouseDown={(event) => event.preventDefault()}
          ref={listbox}
          role="listbox"
          aria-label={label ? `${label} suggestions` : 'Suggestions'}
        >
          {expanded &&
            suggestions.map((suggestion, index) => (
              <li
                aria-selected={index === activeIndex}
                className={clsx(styles.option, index === activeIndex && styles.optionActive)}
                id={optionId(index)}
                // The source may return the same value twice, so the position keeps keys unique.
                key={`${index}-${suggestion}`}
                onClick={() => select(index)}
                role="option"
              >
                {highlightMatch(suggestion, query)}
              </li>
            ))}
        </ul>
      </div>
      <p className="visually-hidden" id={usageHintId}>
        {usageHint}
      </p>
      <div aria-atomic="true" aria-live="polite" className="visually-hidden" role="status">
        {status}
      </div>
    </div>
  )
}
