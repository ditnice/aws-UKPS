import type { FormValue, Question } from '@/lib/dynamic-form/types'

import type { LabelStyle } from './labelStyle'
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
  /**
   * The errors of a text list's or hierarchical select's entries or a group's items, keyed as in
   * `QuestionError.items` (lib/dynamic-form/answers.ts), such as `0.study_name`.
   */
  errorItems?: Record<string, string>
  /** Removes the question's error until it is next validated, such as when the user starts over. */
  clearError: () => void
  /** How the question shows its own label or legend; see `LabelStyle`. */
  labelStyle: LabelStyle
  /**
   * Questions revealed by an option, keyed by option value. Radio controls show them beneath the
   * option while it is chosen.
   */
  revealed?: Partial<Record<string, ReactNode>>
}
