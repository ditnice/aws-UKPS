import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import type { QuestionProps } from '../types'

export function RadioQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
}: QuestionProps<string>) {
  return (
    <FormGroup groupError={error} hint={question.hint ?? undefined} legend={question.label}>
      {question.options?.map((option) => (
        <Radio
          checked={value === option.value}
          error={Boolean(error)}
          key={option.value}
          label={option.label}
          name={name}
          onBlur={onBlur}
          onChange={() => onChange(option.value)}
          value={option.value}
        />
      ))}
    </FormGroup>
  )
}
