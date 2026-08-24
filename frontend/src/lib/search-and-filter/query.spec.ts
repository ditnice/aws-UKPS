import { describe, expect, it } from 'vitest'

import { parseMulti } from './query'

const validValues = ['active', 'inactive'] as const

describe('parseMulti', () => {
  it('returns an empty array for a missing parameter', () => {
    expect(parseMulti(undefined, validValues)).toEqual([])
  })

  it('parses a valid single value', () => {
    expect(parseMulti('active', validValues)).toEqual(['active'])
  })

  it('keeps valid values and removes invalid values', () => {
    expect(parseMulti(['active', 'unknown', 'inactive'], validValues)).toEqual([
      'active',
      'inactive',
    ])
  })
})
