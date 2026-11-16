import { Radio, Radios } from '@/components/Radios/Radios'
import { Select, SelectOption } from '@/components/Select/Select'
import { partsOf } from '@/lib/dynamic-form/dates'
import type { DatePart, DatePrecision, DateValue } from '@/lib/dynamic-form/dates'

import styles from '../DynamicForm.module.scss'

import { DateParts } from './DateParts'

import type { QuestionProps } from '../types'
import type { DatePartsProps } from './DateParts'
import type { ReactNode } from 'react'

const quarters = [
  { value: '1', label: 'January to March' },
  { value: '2', label: 'April to June' },
  { value: '3', label: 'July to September' },
  { value: '4', label: 'October to December' },
]

type ControlsProps = Omit<DatePartsProps, 'name' | 'onBlur' | 'parts'>

/** A quarter of the year as a dropdown, with the year beside it. */
function QuarterAndYear(props: Omit<DatePartsProps, 'onBlur' | 'parts'>) {
  const { 'aria-describedby': describedBy, name, value, onChange, invalid } = props
  return (
    <div className={styles.dateInput}>
      <Select
        aria-describedby={describedBy}
        error={invalid.includes('quarter')}
        label="Quarter"
        name={`${name}-quarter`}
        onChange={(event) => onChange({ ...value, quarter: event.target.value })}
        value={value.quarter}
      >
        <SelectOption value="">Select a quarter</SelectOption>
        {quarters.map((quarter) => (
          <SelectOption key={quarter.value} value={quarter.value}>
            {quarter.label}
          </SelectOption>
        ))}
      </Select>
      <DateParts {...props} parts={['year']} />
    </div>
  )
}

/**
 * A date at the precision the user can give: an estimated quarter or month, or an actual date.
 * Choosing a precision reveals the parts it needs, empty and without the previous error.
 */
export function PrecisionDateQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  errorParts,
  clearError,
}: QuestionProps<DateValue>) {
  const errorId = `${name}-error`
  // Once a precision is chosen, the error sits just above its controls and only the parts it is
  // about are marked as invalid (all of them for an error from the server); until then it is on
  // the radios.
  const reveal = (precision: DatePrecision, controls: (props: ControlsProps) => ReactNode) => {
    const ownError = value.precision === precision ? error : undefined
    const invalid = ownError ? ((errorParts as DatePart[] | undefined) ?? partsOf[precision]) : []
    return (
      <>
        {ownError && (
          <p className="form-group__error-message" id={errorId}>
            <span className="visually-hidden">Error:</span> {ownError}
          </p>
        )}
        {controls({
          'aria-describedby': ownError ? errorId : undefined,
          invalid,
          onChange,
          value,
        })}
      </>
    )
  }
  return (
    // Revalidate only when focus leaves the question, so moving from a newly chosen precision
    // into its parts, or between the parts, does not flag them before they are filled in.
    <div
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) {
          onBlur()
        }
      }}
    >
      <Radios
        error={value.precision ? undefined : error}
        hint={question.hint ?? undefined}
        legend={question.label}
        name={name}
        onChange={(precision) => {
          onChange({
            precision: precision as DatePrecision,
            day: '',
            month: '',
            quarter: '',
            year: '',
          })
          clearError()
        }}
        value={value.precision || null}
      >
        <Radio
          conditional={reveal('EstimatedQuarter', (props) => (
            <QuarterAndYear name={`${name}-quarter`} {...props} />
          ))}
          label="Estimated quarter"
          value="EstimatedQuarter"
        />
        <Radio
          conditional={reveal('EstimatedMonth', (props) => (
            <DateParts name={`${name}-month`} parts={['month', 'year']} {...props} />
          ))}
          label="Estimated month"
          value="EstimatedMonth"
        />
        <Radio
          conditional={reveal('ActualDate', (props) => (
            <DateParts name={`${name}-actual`} parts={['day', 'month', 'year']} {...props} />
          ))}
          label="Actual date"
          value="ActualDate"
        />
      </Radios>
    </div>
  )
}
