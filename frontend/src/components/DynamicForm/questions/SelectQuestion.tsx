import clsx from 'clsx'

import { Select, SelectOption } from '@/components/Select/Select'

import styles from '../DynamicForm.module.scss'

import { QuestionHint } from './QuestionHint'
import { hintId, type QuestionProps } from './types'

export function SelectQuestion({
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
      <Select
        aria-describedby={question.hint ? hintId(question) : undefined}
        disabled={disabled}
        error={Boolean(error)}
        errorMessage={error}
        label={question.label}
        name={question.id}
        onBlur={onBlur}
        onChange={(event) => onChange(event.target.value)}
        value={typeof value === 'string' ? value : ''}
        width="one-third"
      >
        <SelectOption value="">Choose an option</SelectOption>
        {question.options?.map((option) => (
          <SelectOption key={option.value} value={option.value}>
            {option.label}
          </SelectOption>
        ))}
      </Select>
    </div>
  )
}
