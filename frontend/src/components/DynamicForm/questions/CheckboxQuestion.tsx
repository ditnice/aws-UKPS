import clsx from 'clsx'

import { Checkbox } from '@nice-digital/nds-checkbox'
import { FormGroup } from '@nice-digital/nds-form-group'

import styles from '../DynamicForm.module.scss'

import { QuestionHint } from './QuestionHint'

import type { QuestionProps } from './types'

export function CheckboxQuestion({
  question,
  value,
  onChange,
  onBlur,
  error,
  disabled,
  labelAsHeading,
}: QuestionProps) {
  const selected = Array.isArray(value) ? value.map(String) : []

  return (
    <div className={clsx(labelAsHeading && styles.labelAsHeading)}>
      <QuestionHint question={question} />
      <FormGroup groupError={error} legend={question.label} name={question.id}>
        {question.options?.map((option) => (
          <Checkbox
            checked={selected.includes(option.value)}
            disabled={disabled}
            error={Boolean(error)}
            key={option.value}
            label={option.label}
            name={question.id}
            onBlur={onBlur}
            onChange={() =>
              onChange(
                selected.includes(option.value)
                  ? selected.filter((v) => v !== option.value)
                  : [...selected, option.value],
              )
            }
            value={option.value}
          />
        ))}
      </FormGroup>
    </div>
  )
}
