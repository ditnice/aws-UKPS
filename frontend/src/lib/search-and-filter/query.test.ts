import { describe, expect, it } from 'vitest'

import { getNextSortDirection, parseMulti, parseSortDirection } from './query'

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

  it('returns an empty array for empty strings and invalid scalar values', () => {
    expect(parseMulti('', validValues)).toEqual([])
    expect(parseMulti('unknown', validValues)).toEqual([])
  })
})

describe('parseSortDirection', () => {
  it.each([
    ['Ascending', 'Ascending'],
    ['Descending', 'Descending'],
  ])('accepts %s', (input, expected) => {
    expect(parseSortDirection(input)).toBe(expected)
  })

  it.each([undefined, '', 'unknown', 'ascending', ' Ascending '])('rejects %s', (input) => {
    expect(parseSortDirection(input)).toBeUndefined()
  })
})

describe('getNextSortDirection', () => {
  it.each([
    ['Ascending', 'ascending'],
    ['Descending', 'descending'],
  ] as const)('returns %s display direction for a matching column', (sortDirection, expected) => {
    expect(getNextSortDirection({ column: 'email', sortBy: 'email', sortDirection })).toBe(expected)
  })

  it('returns none for a different column or missing sort direction', () => {
    expect(
      getNextSortDirection({ column: 'email', sortBy: 'role', sortDirection: 'Ascending' }),
    ).toBe('none')
    expect(
      getNextSortDirection({ column: 'email', sortBy: 'email', sortDirection: undefined }),
    ).toBe('none')
    expect(
      getNextSortDirection({ column: 'email', sortBy: undefined, sortDirection: 'Ascending' }),
    ).toBe('none')
  })
})
