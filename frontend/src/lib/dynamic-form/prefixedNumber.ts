/**
 * Server messages for a prefixed number; mirror RecordFormMessages.SelectIdType, IdDigitsOnly,
 * IdOutOfRange and IdDigits. All but the first name the number (the question's `numberName`).
 */
export const selectIdTypeMessage = 'Select ID type'
export const idDigitsOnlyMessage = (name: string) => `${name} must be a whole positive number`
export const idOutOfRangeMessage = (name: string) => `${name} must not be longer than 5 digits`
export const idDigitsMessage = (name: string, digits: number) =>
  `${name} must be ${digits} digits long`

/** What the messages call the number when the server does not say; FormValuePrefixesAttribute.Name's default. */
const defaultNumberName = 'ID'

const maxDigits = 5

/** Splits a value into the prefix it starts with, or '' when it starts with none, and the rest. */
export const splitPrefixedNumber = (prefixes: string[], value: string) => {
  const prefix = prefixes.find((candidate) => value.startsWith(candidate)) ?? ''
  return { prefix, number: value.slice(prefix.length) }
}

/**
 * The value the server would save and the message it would reject it with. A prefix with no
 * digits is saved as blank, so it is never an error. `digits` is the exact number of digits
 * needed, or up to 5 when not given; `name` is what the messages call the number. Mirrors
 * PrefixedNumberAnswer.Evaluate.
 */
export const evaluatePrefixedNumber = (
  prefixes: string[],
  value: string | null,
  digits?: number | null,
  name?: string | null,
): { value: string | null; error: string | null } => {
  const numberName = name || defaultNumberName
  if (value === null) {
    return { value: null, error: null }
  }
  const { prefix, number } = splitPrefixedNumber(prefixes, value)
  if (prefix === '') {
    return { value, error: selectIdTypeMessage }
  }
  if (number === '') {
    return { value: null, error: null }
  }
  if (!/^[0-9]+$/.test(number) || /^0+$/.test(number)) {
    return { value, error: idDigitsOnlyMessage(numberName) }
  }
  if (digits) {
    return { value, error: number.length !== digits ? idDigitsMessage(numberName, digits) : null }
  }
  return { value, error: number.length > maxDigits ? idOutOfRangeMessage(numberName) : null }
}

/**
 * Which boxes an error is about: the type for "Select ID type", the number for digit and length
 * errors, both for anything else. `name` is the question's `numberName`.
 */
export const prefixedNumberErrorParts = (error?: string, name?: string | null) => {
  const numberName = name || defaultNumberName
  if (error === selectIdTypeMessage) {
    return { prefix: true, number: false }
  }
  if (
    error === idDigitsOnlyMessage(numberName) ||
    error === idOutOfRangeMessage(numberName) ||
    (error?.startsWith(`${numberName} must be `) && error.endsWith(' digits long'))
  ) {
    return { prefix: false, number: true }
  }
  return { prefix: Boolean(error), number: Boolean(error) }
}
