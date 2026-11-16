import type { FormValue, Question } from '@/lib/dynamic-form/types'

import type { ReactNode } from 'react'

/** The props every question component takes. */
export type QuestionProps<TValue extends FormValue = FormValue> = {
  question: Question
  /** The field name; also the ID of the first input unless the control says otherwise. */
  name: string
  value: TValue
  onChange: (value: TValue) => void
  onBlur: () => void
  error?: string
  /**
   * The inputs the error is about, for a question entered in several parts such as a date; all
   * of them when not given, as for an error from the server.
   */
  errorParts?: string[]
  /** Removes the question's error until it is next validated, such as when the user starts over. */
  clearError: () => void
  /** True on a single-question page, where the label is already the page heading. */
  labelAsHeading: boolean
  /**
   * Questions revealed by an option, keyed by option value. Radio controls show them beneath the
   * option while it is chosen.
   */
  revealed?: Partial<Record<string, ReactNode>>
}
