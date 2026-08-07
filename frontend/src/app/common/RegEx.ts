const PHONE_NUMBER_REGEX = /^(\+|0)[1-9][0-9 \-\(\)\.]{7,32}$/

export function isValidPhoneNumber(value: string): boolean {
  return PHONE_NUMBER_REGEX.test(value.trim())
}
