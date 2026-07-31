const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const PHONE_NUMBER_REGEX = /^(\+|0)[1-9][0-9 \-\(\)\.]{7,32}$/

export function isValidEmail(value: string): boolean {
  return EMAIL_REGEX.test(value.trim())
}

export function isValidPhoneNumber(value: string): boolean {
  return PHONE_NUMBER_REGEX.test(value.trim())
}
