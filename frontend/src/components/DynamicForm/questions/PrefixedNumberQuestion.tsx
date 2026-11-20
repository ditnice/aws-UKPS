import { FormGroup } from '@/components/FormGroup/FormGroup'
import { Input } from '@/components/Input/Input'
import { Select, SelectOption } from '@/components/Select/Select'
import { prefixedNumberErrorParts, splitPrefixedNumber } from '@/lib/dynamic-form/prefixedNumber'

import styles from '../DynamicForm.module.scss'

import type { QuestionProps } from '../types'

/**
 * A dropdown of prefixes beside a box for the digits; the value is the two joined. With only one
 * prefix there is nothing to choose, so it is shown as a fixed prefix on the box instead.
 */
export function PrefixedNumberQuestion(props: QuestionProps<string>) {
  return (props.question.prefixes ?? []).length === 1 ? (
    <FixedPrefixNumber {...props} />
  ) : (
    <ChosenPrefixNumber {...props} />
  )
}

/** A box for the digits after a single, fixed prefix, such as "NCT". */
function FixedPrefixNumber({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  labelAsHeading,
}: QuestionProps<string>) {
  const prefix = question.prefixes![0]
  const { number } = splitPrefixedNumber([prefix], value)
  // A whole ID pasted with its prefix, in any case, is not prefixed again: "nct01234567" is saved
  // as "NCT01234567".
  const changeNumber = (typed: string) =>
    onChange(
      prefix +
        (typed.slice(0, prefix.length).toUpperCase() === prefix.toUpperCase()
          ? typed.slice(prefix.length)
          : typed),
    )

  return (
    <>
      {labelAsHeading && (
        <label className="visually-hidden" htmlFor={name}>
          {question.label}
        </label>
      )}
      <Input
        autoComplete="off"
        error={Boolean(error)}
        errorMessage={error}
        hint={question.hint ?? undefined}
        inputMode="numeric"
        label={labelAsHeading ? null : question.label}
        name={name}
        onBlur={onBlur}
        onChange={(event) => changeNumber(event.target.value)}
        prefix={prefix}
        value={number}
        width={10}
      />
    </>
  )
}

function ChosenPrefixNumber({
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
  const errorParts = prefixedNumberErrorParts(error, question.numberName)
  // A whole ID pasted or typed into the number box replaces both parts rather than being
  // appended after the chosen prefix, so "GID-TA123" never becomes "GID-TAGID-TA123".
  const changeNumber = (typed: string) =>
    onChange(splitPrefixedNumber(prefixes, typed).prefix ? typed : prefix + typed)

  return (
    <FormGroup error={error} hint={question.hint ?? undefined} legend={question.label}>
      <div className={styles.prefixedNumber}>
        <Select
          aria-label={placeholder}
          error={errorParts.prefix}
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
          error={errorParts.number}
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
