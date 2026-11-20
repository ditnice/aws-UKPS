import { Input } from '@/components/Input/Input'

import { labelSizeFor } from '../labelStyle'

import type { QuestionProps } from '../types'

export function TextQuestion({
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
      <Input
        error={Boolean(error)}
        errorMessage={error}
        hint={question.hint ?? undefined}
        label={labelAsHeading ? null : question.label}
        labelSize={labelSizeFor(labelStyle)}
        name={name}
        onBlur={onBlur}
        onChange={(event) => onChange(event.target.value)}
        value={value}
        width="two-thirds"
      />
    </>
  )
}
