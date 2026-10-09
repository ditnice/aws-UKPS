import clsx from 'clsx'

import { Textarea } from '@/components/Textarea/Textarea'

import styles from '../DynamicForm.module.scss'

import { QuestionHint } from './QuestionHint'
import { hintId, type QuestionProps } from './types'

import type { ChangeEvent } from 'react'

export function TextareaQuestion({
  question,
  value,
  onChange,
  onBlur,
  error,
  disabled,
  labelAsHeading,
}: QuestionProps) {
  return (
    <div className={clsx(labelAsHeading && styles.labelAsHeading)}>
      <QuestionHint question={question} />
      <Textarea
        aria-describedby={question.hint ? hintId(question) : undefined}
        disabled={disabled}
        error={Boolean(error)}
        errorMessage={error}
        label={question.label}
        name={question.id}
        onBlur={onBlur}
        onChange={(event: ChangeEvent<HTMLTextAreaElement>) => onChange(event.target.value)}
        value={typeof value === 'string' ? value : ''}
        width="full"
      />
    </div>
  )
}
