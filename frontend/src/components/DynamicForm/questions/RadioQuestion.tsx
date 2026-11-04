import { Radio, Radios } from '@/components/Radios/Radios'

import styles from '../DynamicForm.module.scss'

import type { QuestionProps } from '../types'

export function RadioQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
}: QuestionProps<string>) {
  // Labels are bold across the whole group once any option has a hint, matching the designs.
  const hasOptionHints = question.options?.some((option) => option.hint) ?? false

  return (
    <Radios
      className={hasOptionHints ? styles.boldLabels : undefined}
      error={error}
      hint={question.hint ?? undefined}
      legend={question.label}
      name={name}
      onBlur={onBlur}
      onChange={onChange}
      value={value ?? null}
    >
      {question.options?.map((option) => (
        <Radio
          hint={option.hint ?? undefined}
          key={option.value}
          label={option.label}
          value={option.value}
        />
      ))}
    </Radios>
  )
}
