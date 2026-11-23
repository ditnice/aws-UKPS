import { describe, expect, it } from 'vitest'

import type { FormInputWidth } from '@/client/generated'

import { fluidInputWidthOf, inputWidthOf } from './inputWidth'

describe('inputWidthOf', () => {
  it.each([
    ['Full', 'full'],
    ['ThreeQuarters', 'three-quarters'],
    ['TwoThirds', 'two-thirds'],
    ['OneHalf', 'one-half'],
    ['OneThird', 'one-third'],
    ['OneQuarter', 'one-quarter'],
    ['Chars2', 2],
    ['Chars3', 3],
    ['Chars4', 4],
    ['Chars5', 5],
    ['Chars10', 10],
    ['Chars20', 20],
    ['Chars30', 30],
  ] as const)('reads %s as %s', (width, expected) => {
    expect(inputWidthOf(width)).toBe(expected)
  })

  it.each([undefined, null, 'Default' as FormInputWidth])('leaves %s to the default', (width) => {
    expect(inputWidthOf(width)).toBeUndefined()
  })
})

describe('fluidInputWidthOf', () => {
  it('reads a share of the container', () => {
    expect(fluidInputWidthOf('Full')).toBe('full')
  })

  it('ignores a character count', () => {
    expect(fluidInputWidthOf('Chars20')).toBeUndefined()
  })
})
