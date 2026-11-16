import { Checkbox, Checkboxes } from '@/components/Checkboxes/Checkboxes'

import type { QuestionProps } from '../types'

export function CheckboxQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
}: QuestionProps<string[]>) {
  return (
    <Checkboxes
      error={error}
      hint={question.hint ?? undefined}
      legend={question.label}
      name={name}
      value={value}
      // Revalidate on every tick, not only on blur, so an error clears as soon as one is ticked.
      onChange={(next) => {
        onChange(next)
        onBlur()
      }}
      onBlur={onBlur}
    >
      {question.options?.map((option) => (
        <Checkbox key={option.value} label={option.label} value={option.value} />
      ))}
    </Checkboxes>
  )
}
