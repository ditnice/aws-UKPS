import clsx from 'clsx'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import styles from '../DynamicForm.module.scss'

import { QuestionHint } from './QuestionHint'

import type { QuestionProps } from './types'

export function RadioQuestion({
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
      <FormGroup groupError={error} legend={question.label} name={question.id}>
        {question.options?.map((option) => (
          <Radio
            checked={value === option.value}
            disabled={disabled}
            error={Boolean(error)}
            key={option.value}
            label={option.label}
            onBlur={onBlur}
            onChange={() => onChange(option.value)}
            value={option.value}
          />
        ))}
      </FormGroup>
    </div>
  )
}
