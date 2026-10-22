import { Textarea } from '@/components/Textarea/Textarea'

import type { QuestionProps } from '../types'
import type { ChangeEvent } from 'react'

export function TextareaQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  disabled,
}: QuestionProps<string>) {
  return (
    <Textarea
      disabled={disabled}
      error={Boolean(error)}
      errorMessage={error}
      hint={question.hint ?? undefined}
      label={question.label}
      name={name}
      onBlur={onBlur}
      onChange={(event: ChangeEvent<HTMLTextAreaElement>) => onChange(event.target.value)}
      value={value}
    />
  )
}
