import { Checkbox } from '@nice-digital/nds-checkbox'
import { FormGroup } from '@nice-digital/nds-form-group'

import type { QuestionProps } from '../types'

export function CheckboxQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
}: QuestionProps<string[]>) {
  const toggle = (optionValue: string, checked: boolean) =>
    onChange(checked ? [...value, optionValue] : value.filter((v) => v !== optionValue))

  return (
    <FormGroup groupError={error} hint={question.hint ?? undefined} legend={question.label}>
      {question.options?.map((option) => (
        <Checkbox
          checked={value.includes(option.value)}
          error={Boolean(error)}
          key={option.value}
          label={option.label}
          name={name}
          onBlur={onBlur}
          onChange={(event: { target: { checked: boolean } }) =>
            toggle(option.value, event.target.checked)
          }
          value={option.value}
        />
      ))}
    </FormGroup>
  )
}
