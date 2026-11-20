import { Select, SelectOption } from '@/components/Select/Select'

import { labelSizeFor } from '../labelStyle'

import type { QuestionProps } from '../types'

export function SelectQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  labelStyle,
}: QuestionProps<string>) {
  const labelAsHeading = labelStyle === 'heading'
  return (
    <>
      {labelAsHeading && (
        <label className="visually-hidden" htmlFor={name}>
          {question.label}
        </label>
      )}
      <Select
        error={Boolean(error)}
        errorMessage={error}
        hint={question.hint ?? undefined}
        label={labelAsHeading ? null : question.label}
        labelSize={labelSizeFor(labelStyle)}
        name={name}
        onBlur={onBlur}
        onChange={(event) => onChange(event.target.value)}
        value={value}
        width="one-third"
      >
        <SelectOption value="">{question.placeholder ?? 'Select an option'}</SelectOption>
        {question.options?.map((option) => (
          <SelectOption key={option.value} value={option.value}>
            {option.label}
          </SelectOption>
        ))}
      </Select>
    </>
  )
}
