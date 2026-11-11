import { FormGroup } from '@/components/FormGroup/FormGroup'
import { Input } from '@/components/Input/Input'
import { Select, SelectOption } from '@/components/Select/Select'
import { splitPrefixedNumber } from '@/lib/dynamic-form/prefixedNumber'

import styles from '../DynamicForm.module.scss'

import type { QuestionProps } from '../types'

/** A dropdown of prefixes beside a box for the digits; the value is the two joined. */
export function PrefixedNumberQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
}: QuestionProps<string>) {
  const prefixes = question.prefixes ?? []
  const { prefix, number } = splitPrefixedNumber(prefixes, value)
  const placeholder = question.placeholder ?? 'Select an option'
  // A whole ID pasted or typed into the number box replaces both parts rather than being
  // appended after the chosen prefix, so "GID-TA123" never becomes "GID-TAGID-TA123".
  const changeNumber = (typed: string) =>
    onChange(splitPrefixedNumber(prefixes, typed).prefix ? typed : prefix + typed)

  return (
    <FormGroup error={error} hint={question.hint ?? undefined} legend={question.label}>
      <div className={styles.prefixedNumber}>
        <Select
          aria-label={placeholder}
          error={Boolean(error)}
          label={null}
          name={name}
          onBlur={onBlur}
          onChange={(event) => onChange(event.target.value + number)}
          value={prefix}
          width={10}
        >
          <SelectOption value="">{placeholder}</SelectOption>
          {prefixes.map((option) => (
            <SelectOption key={option} value={option}>
              {option}
            </SelectOption>
          ))}
        </Select>
        <Input
          aria-label="Number"
          autoComplete="off"
          error={Boolean(error)}
          inputMode="numeric"
          label={null}
          name={`${name}-number`}
          onBlur={onBlur}
          onChange={(event) => changeNumber(event.target.value)}
          value={number}
          width={20}
        />
      </div>
    </FormGroup>
  )
}
