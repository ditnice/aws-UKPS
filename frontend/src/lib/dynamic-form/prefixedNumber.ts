/** Server messages for a prefixed number; mirror RecordFormMessages.SelectIdType and IdDigitsOnly. */
export const selectIdTypeMessage = 'Select ID type'
export const idDigitsOnlyMessage = 'ID must only contain numbers'

/** Splits a value into the prefix it starts with, or '' when it starts with none, and the rest. */
export const splitPrefixedNumber = (prefixes: string[], value: string) => {
  const prefix = prefixes.find((candidate) => value.startsWith(candidate)) ?? ''
  return { prefix, number: value.slice(prefix.length) }
}

/**
 * The value the server would save and the message it would reject it with. A prefix with no
 * digits is saved as blank, so it is never an error. Mirrors PrefixedNumberAnswer.Evalua
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
  return { value, error: /^[0-9]+$/.test(number) ? null : idDigitsOnlyMessage }
}
