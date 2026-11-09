import { Input } from '@/components/Input/Input'
import { Radio, Radios } from '@/components/Radios/Radios'
import type { DatePrecision, DateValue } from '@/lib/dynamic-form/dates'

import styles from '../DynamicForm.module.scss'

import type { QuestionProps } from '../types'

type Part = 'day' | 'month' | 'year'

const partInputs: Record<Part, { label: string; width: 2 | 4 }> = {
  day: { label: 'Day', width: 2 },
  month: { label: 'Month', width: 2 },
  year: { label: 'Year', width: 4 },
}

const quarters = [
  { value: '1', label: 'January to March' },
  { value: '2', label: 'April to June' },
  { value: '3', label: 'July to September' },
  { value: '4', label: 'October to December' },
]

type PartsProps = {
  /** Prefix for the inputs' names and IDs, for example `q0-actual`. */
  name: string
  value: DateValue
  onChange: (value: DateValue) => void
  onBlur: () => void
  invalid: boolean
}

/** Day, month and year inputs side by side, following the GOV.UK date input pattern. */
function DateParts({
  name,
  parts,
  value,
  onChange,
  onBlur,
  invalid,
}: PartsProps & { parts: Part[] }) {
  return (
    <div className={styles.dateInput}>
      {parts.map((part) => (
        <Input
          aria-invalid={invalid || undefined}
          id={`${name}-${part}`}
          inputMode="numeric"
          key={part}
          label={partInputs[part].label}
          name={`${name}-${part}`}
          onBlur={onBlur}
          onChange={(event) => onChange({ ...value, [part]: event.target.value })}
          value={value[part]}
          width={partInputs[part].width}
        />
      ))}
    </div>
  )
}

/** A quarter of the year as radios, with the year beneath. */
function QuarterAndYear(props: PartsProps) {
  const { name, value, onChange, onBlur } = props
  return (
    <>
      <Radios
        legend="Quarter"
        name={`${name}-quarter`}
        onBlur={onBlur}
        onChange={(quarter) => onChange({ ...value, quarter })}
        value={value.quarter || null}
      >
        {quarters.map((quarter) => (
          <Radio key={quarter.value} label={quarter.label} value={quarter.value} />
        ))}
      </Radios>
      <DateParts {...props} parts={['year']} />
    </>
  )
}

/**
 * A date at the precision the user can give: an estimated quarter or month, or an actual date.
 * Choosing a precision reveals the parts it needs; parts typed under another precision are kept
 * but not sent.
 */
export function PrecisionDateQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
}: QuestionProps<DateValue>) {
  const parts = { onBlur, onChange, value, invalid: Boolean(error) }
  return (
    <Radios
      error={error}
      hint={question.hint ?? undefined}
      legend={question.label}
      name={name}
      onBlur={onBlur}
      onChange={(precision) => onChange({ ...value, precision: precision as DatePrecision })}
      value={value.precision || null}
    >
      <Radio
        conditional={<QuarterAndYear name={`${name}-quarter`} {...parts} />}
        label="Estimated quarter"
        value="EstimatedQuarter"
      />
      <Radio
        conditional={<DateParts name={`${name}-month`} parts={['month', 'year']} {...parts} />}
        label="Estimated month"
        value="EstimatedMonth"
      />
      <Radio
        conditional={
          <DateParts name={`${name}-actual`} parts={['day', 'month', 'year']} {...parts} />
        }
        label="Actual date"
        value="ActualDate"
      />
    </Radios>
  )
}
