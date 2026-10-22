import { Input } from '@/components/Input/Input'

import type { QuestionProps } from '../types'

export function TextQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  disabled,
  labelAsHeading,
}: QuestionProps<string>) {
  return (
    <>
      {labelAsHeading && (
        <label className="visually-hidden" htmlFor={name}>
          {question.label}
        </label>
      )}
      <Input
        disabled={disabled}
        error={Boolean(error)}
        errorMessage={error}
        hint={question.hint ?? undefined}
        label={labelAsHeading ? null : question.label}
        name={name}
        onBlur={onBlur}
        onChange={(event) => onChange(event.target.value)}
        value={value}
        width="two-thirds"
      />
    </>
  )
}
