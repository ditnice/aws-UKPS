import { FormGroup } from '@/components/FormGroup/FormGroup'
import type { DateValue } from '@/lib/dynamic-form/dates'

import { labelSizeFor } from '../labelStyle'

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
  labelStyle,
}: QuestionProps<DateValue>) {
  return (
    <FormGroup
      error={error}
      hint={question.hint ?? undefined}
      identifier={name}
      legend={question.label}
      legendSize={labelSizeFor(labelStyle)}
    >
      <DateParts
        invalid={error ? ['month', 'year'] : []}
        name={name}
        onBlur={onBlur}
        onChange={onChange}
        parts={['month', 'year']}
        value={value}
      />
    </FormGroup>
  )
}
