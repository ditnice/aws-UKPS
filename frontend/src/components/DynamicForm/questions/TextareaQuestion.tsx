import { Textarea } from '@/components/Textarea/Textarea'

import { labelSizeFor } from '../labelStyle'

import type { QuestionProps } from '../types'
import type { ChangeEvent } from 'react'

export function TextareaQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  labelStyle,
}: QuestionProps<string>) {
  return (
    <Textarea
      error={Boolean(error)}
      errorMessage={error}
      hint={question.hint ?? undefined}
      label={question.label}
      labelSize={labelSizeFor(labelStyle)}
      name={name}
      onBlur={onBlur}
      onChange={(event: ChangeEvent<HTMLTextAreaElement>) => onChange(event.target.value)}
      value={value}
    />
  )
}
