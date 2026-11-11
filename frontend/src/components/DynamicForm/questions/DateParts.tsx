import { Input } from '@/components/Input/Input'
import type { DateValue } from '@/lib/dynamic-form/dates'

import styles from '../DynamicForm.module.scss'

type Part = 'day' | 'month' | 'year'

const partInputs: Record<Part, { label: string; width: 2 | 5 }> = {
  day: { label: 'Day', width: 2 },
  month: { label: 'Month', width: 2 },
  year: { label: 'Year', width: 5 },
}

export type DatePartsProps = {
  /** Prefix for the inputs' names and IDs, for example `q0-actual`. */
  name: string
  value: DateValue
  onChange: (value: DateValue) => void
  onBlur?: () => void
  /** Styles the inputs as invalid; the message itself is shown by the caller. */
  error: boolean
  /** The ID of an error message shown outside a fieldset that would otherwise describe them. */
  'aria-describedby'?: string
  parts: Part[]
}

/** Day, month and year inputs side by side, following the GOV.UK date input pattern. */
export function DateParts({
  'aria-describedby': describedBy,
  name,
  parts,
  value,
  onChange,
  onBlur,
  error,
}: DatePartsProps) {
  return (
    <div className={styles.dateInput}>
      {parts.map((part) => (
        <Input
          aria-describedby={describedBy}
          error={error}
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
