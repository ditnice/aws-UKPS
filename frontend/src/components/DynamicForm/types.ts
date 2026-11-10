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
  /** True on a single-question page, where the label is already the page heading. */
  labelAsHeading: boolean
  /**
   * The ID of the page heading that already holds the label, set only on a single question page.
   * Controls with a group name point `aria-labelledby` at it instead of repeating the label.
   */
  headingId?: string
  /**
   * Questions revealed by an option, keyed by option value. Radio controls show them beneath the
   * option while it is chosen.
   */
  revealed?: Partial<Record<string, ReactNode>>
}
