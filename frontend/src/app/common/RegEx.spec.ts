import { describe, expect, it } from 'vitest'

import { isValidPhoneNumber } from './RegEx'

describe('isValidPhoneNumber', () => {
  it.each(['01632 960 001', '07700 900 982', '+44 808 157 0192', ' 01632 960 001 '])(
    'returns true for a valid phone number %s',
    (value) => {
      expect(isValidPhoneNumber(value)).toBe(true)
    },
  )

  it.each(['', '123', 'not-a-phone-number', '00 1234 5678'])(
    'returns false for an invalid phone number %s',
    (value) => {
      expect(isValidPhoneNumber(value)).toBe(false)
    },
  )
})
