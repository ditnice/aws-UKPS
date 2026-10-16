import type { FormOptionDto } from '@/client/generated'

import { CheckboxQuestion } from './CheckboxQuestion'

import type { QuestionProps } from './types'

/**
 * The contract for the searchable multi-select used by checkbox questions with
 * `display: "combobox"` (e.g. therapeutic area). The value is the selected option values.
 */
export type ComboboxProps = {
  id: string
  label: string
  hint?: string | null
  options: FormOptionDto[]
  value: string[]
  onChange: (value: string[]) => void
  onBlur: () => void
  error?: string
  disabled: boolean
}

/**
 * Placeholder until the combobox component is built: renders a checkbox list. To plug the
 * combobox in, render it here with `ComboboxProps`.
 */
export function ComboboxSlot(props: QuestionProps) {
  return <CheckboxQuestion {...props} />
}
