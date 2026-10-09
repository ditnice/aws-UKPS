'use client'

import { useEffect, useRef } from 'react'

import styles from './ErrorSummary.module.scss'

export type ErrorSummaryItem = {
  /** The ID of the element to move focus to, or omit for an error with no field. */
  targetId?: string
  message: string
}

export type ErrorSummaryProps = {
  errors: ErrorSummaryItem[]
  title?: string
  /** Changes whenever the summary should take focus again, e.g. the submission count. */
  focusKey?: unknown
}

/**
 * Lists a form's errors at the top of the page, linking each to its field. Takes focus when
 * shown so keyboard and screen reader users hear what went wrong.
 */
export function ErrorSummary({
  errors,
  title = 'There is a problem',
  focusKey,
}: ErrorSummaryProps) {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    ref.current?.focus()
  }, [focusKey])

  if (errors.length === 0) {
    return null
  }

  return (
    <div
      aria-labelledby="error-summary-title"
      className={styles.summary}
      data-component="error-summary"
      ref={ref}
      role="alert"
      tabIndex={-1}
    >
      <h2 className={styles.title} id="error-summary-title">
        {title}
      </h2>
      <ul className={styles.list}>
        {errors.map((error, index) => (
          <li key={`${error.targetId ?? ''}-${index}`}>
            {error.targetId ? <a href={`#${error.targetId}`}>{error.message}</a> : error.message}
          </li>
        ))}
      </ul>
    </div>
  )
}
