import { FormGroup } from '@/components/FormGroup/FormGroup'
import type { DateValue } from '@/lib/dynamic-form/dates'

import { DateParts } from './DateParts'

import type { QuestionProps } from '../types'

/** The month and year of an event that has happened, as month and year inputs. */
export function ActualMonthQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
}: QuestionProps<DateValue>) {
  return (
    <FormGroup
      error={error}
      hint={question.hint ?? undefined}
      identifier={name}
      legend={question.label}
    >
      <DateParts
        invalid={Boolean(error)}
        name={name}
        onBlur={onBlur}
        onChange={onChange}
        parts={['month', 'year']}
        value={value}
      />
    </FormGroup>
  )
}
