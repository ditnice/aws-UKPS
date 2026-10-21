import { describe, expect, it } from 'vitest'

import { parsePositiveInteger } from './valueParsing'

describe('parsePositiveInteger', () => {
  it('returns the number for a positive integer', () => {
    expect(parsePositiveInteger('1')).toBe(1)
    expect(parsePositiveInteger('123')).toBe(123)
  })

  it('returns the number for the largest safe integer', () => {
    expect(parsePositiveInteger(String(Number.MAX_SAFE_INTEGER))).toBe(Number.MAX_SAFE_INTEGER)
  })

  it('returns null for zero', () => {
    expect(parsePositiveInteger('0')).toBeNull()
  })

  it('returns null for negative numbers', () => {
    expect(parsePositiveInteger('-1')).toBeNull()
  })

  it('returns null for values with leading zeros', () => {
    expect(parsePositiveInteger('01')).toBeNull()
  })

  it('returns null for non-integer values', () => {
    expect(parsePositiveInteger('1.5')).toBeNull()
    expect(parsePositiveInteger('1e3')).toBeNull()
    expect(parsePositiveInteger('abc')).toBeNull()
    expect(parsePositiveInteger(' 1 ')).toBeNull()
    expect(parsePositiveInteger('')).toBeNull()
  })

  it('returns null for integers larger than the largest safe integer', () => {
    expect(parsePositiveInteger(String(Number.MAX_SAFE_INTEGER + 2))).toBeNull()
  })
})
