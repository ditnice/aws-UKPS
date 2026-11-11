/** Server messages for a prefixed number; mirror RecordFormMessages.SelectIdType, IdDigitsOnly and IdOutOfRange. */
export const selectIdTypeMessage = 'Select ID type'
export const idDigitsOnlyMessage = 'ID must be a whole positive number'
export const idOutOfRangeMessage = 'ID must not be longer than 5 digits'

/** Splits a value into the prefix it starts with, or '' when it starts with none, and the rest. */
export const splitPrefixedNumber = (prefixes: string[], value: string) => {
  const prefix = prefixes.find((candidate) => value.startsWith(candidate)) ?? ''
  return { prefix, number: value.slice(prefix.length) }
}

/**
 * The value the server would save and the message it would reject it with. A prefix with no
 * digits is saved as blank, so it is never an error. Mirrors PrefixedNumberAnswer.Evaluate.
 */
export const evaluatePrefixedNumber = (
  prefixes: string[],
  value: string | null,
): { value: string | null; error: string | null } => {
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
  if (!/^[0-9]+$/.test(number)) {
    return { value, error: idDigitsOnlyMessage }
  }
  const outOfRange = number.length > 5 || /^0+$/.test(number)
  return { value, error: outOfRange ? idOutOfRangeMessage : null }
}
