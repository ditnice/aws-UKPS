import { Checkbox, Checkboxes } from '@/components/Checkboxes/Checkboxes'

import { HintText } from '../HintText'
import { labelSizeFor } from '../labelStyle'

import type { QuestionProps } from '../types'

export function CheckboxQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  labelStyle,
}: QuestionProps<string[]>) {
  return (
    <Checkboxes
      error={error}
      hint={question.hint ? <HintText text={question.hint} /> : undefined}
      legend={question.label}
      legendSize={labelSizeFor(labelStyle)}
      name={name}
      value={value}
      // Mark the field as blurred on every tick. Forms validate on submit only, so this does not
      // re-check the answer.
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
