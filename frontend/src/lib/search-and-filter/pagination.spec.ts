import { describe, expect, it } from 'vitest'

import { defaultPageSize, parsePage, parsePageSize } from './pagination'

describe('pagination query parsing', () => {
  it('parses positive integer page numbers', () => {
    expect(parsePage('3')).toBe(3)
  })

  it.each([undefined, '', '0', '-1', '1.5', 'invalid'])('defaults an invalid page: %s', (page) => {
    expect(parsePage(page)).toBe(1)
  })

  it.each(['10', '25', '50'])('parses an allowed page size: %s', (pageSize) => {
    expect(parsePageSize(pageSize)).toBe(Number(pageSize))
  })

  it.each([undefined, '', '20', '-10', 'invalid'])(
    'defaults an invalid page size: %s',
    (pageSize) => {
      expect(parsePageSize(pageSize)).toBe(defaultPageSize)
    },
  )
})
