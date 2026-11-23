import { FormGroup } from '@/components/FormGroup/FormGroup'
import type { DateValue } from '@/lib/dynamic-form/dates'

import { labelSizeFor } from '../labelStyle'

import { DateParts } from './DateParts'

import type { QuestionProps } from '../types'

/** The day, month and year of an event that has happened, as day, month and year inputs. */
export function ActualDateQuestion({
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
        invalid={error ? ['day', 'month', 'year'] : []}
        name={name}
        onBlur={onBlur}
        onChange={onChange}
        parts={['day', 'month', 'year']}
        value={value}
      />
    </FormGroup>
  )
}
