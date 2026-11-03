import { Radio, Radios } from '@/components/Radios/Radios'

import type { QuestionProps } from '../types'

export function RadioQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  revealed,
}: QuestionProps<string>) {
  return (
    <Radios
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
          conditional={revealed?.[option.value]}
          key={option.value}
          label={option.label}
          value={option.value}
        />
      ))}
    </Radios>
  )
}
