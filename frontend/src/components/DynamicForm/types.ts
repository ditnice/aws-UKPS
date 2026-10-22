import type { FormValue, Question } from '@/lib/dynamic-form/types'

/** The props every question component takes. */
export type QuestionProps<TValue extends FormValue = FormValue> = {
  question: Question
  /** The field name; also the ID of the first input unless the control says otherwise. */
  name: string
  value: TValue
  onChange: (value: TValue) => void
  onBlur: () => void
  error?: string
  disabled: boolean
  /** True on a single-question page, where the label is already the page heading. */
  labelAsHeading: boolean
}
