import { describe, expect, it } from 'vitest'

import {
  formatEnum,
  formatEnumList,
  formatList,
  formatRegulatoryDate,
  formatText,
  notProvidedText,
  yesNoUnknownLabels,
} from './recordValueFormatters'

describe('formatText', () => {
  it.each([null, undefined, '', '   '])('returns the fallback for %j', (value) => {
    expect(formatText(value)).toBe(notProvidedText)
  })

  it('returns the value when provided', () => {
    expect(formatText('Hepatitis C')).toBe('Hepatitis C')
  })
})

describe('formatEnum', () => {
  it('returns the label for the value', () => {
    expect(formatEnum('Yes', yesNoUnknownLabels)).toBe('Yes')
  })

  it('returns the fallback when the value is missing', () => {
    expect(formatEnum(null, yesNoUnknownLabels)).toBe(notProvidedText)
  })
})

describe('formatEnumList', () => {
  it('joins the labels for each value', () => {
    expect(formatEnumList(['Yes', 'No'], yesNoUnknownLabels)).toBe('Yes, No')
  })

  it('returns the fallback for an empty list', () => {
    expect(formatEnumList([], yesNoUnknownLabels)).toBe(notProvidedText)
  })
})

describe('formatList', () => {
  it('returns the fallback when the list is missing', () => {
    expect(formatList(undefined)).toBe(notProvidedText)
  })
})

describe('formatRegulatoryDate', () => {
  it('returns the fallback when the date is missing', () => {
    expect(formatRegulatoryDate(null)).toBe(notProvidedText)
  })

  it.each([
    ['EstimatedQuarter', '2027-01-01', 'Q1 2027 (estimated)'],
    ['EstimatedQuarter', '2027-12-01', 'Q4 2027 (estimated)'],
    ['EstimatedMonth', '2027-03-01', 'March 2027 (estimated)'],
    ['ActualDate', '2027-03-12', '12 March 2027'],
  ] as const)('formats %s %s as %s', (datePrecision, dateValue, expected) => {
    expect(formatRegulatoryDate({ dateValue, datePrecision, isConfidential: false })).toBe(expected)
  })

  it('marks confidential dates', () => {
    expect(
      formatRegulatoryDate({
        dateValue: '2027-03-12',
        datePrecision: 'ActualDate',
        isConfidential: true,
      }),
    ).toBe('12 March 2027 (confidential)')
  })
})
