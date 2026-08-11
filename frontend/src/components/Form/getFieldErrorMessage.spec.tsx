import { describe, expect, it } from 'vitest'

import { getFieldErrorMessage } from './getFieldErrorMessage'

describe('getFieldErrorMessage', () => {
  it('returns undefined when there are no errors', () => {
    expect(getFieldErrorMessage([])).toBeUndefined()
  })

  it('returns a string error', () => {
    expect(getFieldErrorMessage(['Enter a value'])).toBe('Enter a value')
  })

  it('returns the message from a standard schema issue', () => {
    expect(getFieldErrorMessage([{ message: 'Enter a valid value', path: ['field'] }])).toBe(
      'Enter a valid value',
    )
  })

  it('returns undefined for unsupported error shapes', () => {
    expect(getFieldErrorMessage([{ error: 'Enter a value' }])).toBeUndefined()
    expect(getFieldErrorMessage([123])).toBeUndefined()
  })
})
