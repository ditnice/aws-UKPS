import { describe, expect, it } from 'vitest'

import type { ReferenceDataDto } from '@/client/generated'

import {
  formatEnum,
  formatEnumList,
  formatList,
  formatReferenceData,
  formatReferenceDataList,
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

  it.each([null, undefined])('returns the fallback for %j', (value) => {
    expect(formatEnum(value, yesNoUnknownLabels)).toBe(notProvidedText)
  })
})

describe('formatEnumList', () => {
  it('joins the labels for each value', () => {
    expect(formatEnumList(['Yes', 'No'], yesNoUnknownLabels)).toBe('Yes, No')
  })

  it.each([null, undefined, []])('returns the fallback for %j', (values) => {
    expect(formatEnumList(values, yesNoUnknownLabels)).toBe(notProvidedText)
  })
})

describe('formatList', () => {
  it.each([null, undefined, []])('returns the fallback for %j', (values) => {
    expect(formatList(values)).toBe(notProvidedText)
  })

  it('joins provided values in their original order', () => {
    expect(formatList(['First', 'Second'])).toBe('First, Second')
  })
})

describe('formatReferenceData', () => {
  it.each([null, undefined])('returns the fallback for %j', (value) => {
    expect(formatReferenceData(value)).toBe(notProvidedText)
  })

  it('returns the reference label', () => {
    const value = { id: 1, label: 'Reference label' } satisfies ReferenceDataDto

    expect(formatReferenceData(value)).toBe('Reference label')
  })
})

describe('formatReferenceDataList', () => {
  it.each([null, undefined, []])('returns the fallback for %j', (values) => {
    expect(formatReferenceDataList(values)).toBe(notProvidedText)
  })

  it('joins reference labels in their original order', () => {
    const values = [
      { id: 1, label: 'First' },
      { id: 2, label: 'Second' },
    ] satisfies ReferenceDataDto[]

    expect(formatReferenceDataList(values)).toBe('First, Second')
  })
})

describe('formatRegulatoryDate', () => {
  it.each([null, undefined])('returns the fallback for %j', (value) => {
    expect(formatRegulatoryDate(value)).toBe(notProvidedText)
  })

  it.each([
    ['EstimatedQuarter', '2027-01-01', 'Q1 2027 (estimated)'],
    ['EstimatedQuarter', '2027-04-01', 'Q2 2027 (estimated)'],
    ['EstimatedQuarter', '2027-07-01', 'Q3 2027 (estimated)'],
    ['EstimatedQuarter', '2027-12-01', 'Q4 2027 (estimated)'],
    ['EstimatedMonth', '2027-03-01', 'March 2027 (estimated)'],
    ['ActualDate', '2027-03-12', '12 March 2027'],
  ] as const)('formats %s %s as %s', (datePrecision, dateValue, expected) => {
    expect(formatRegulatoryDate({ dateValue, datePrecision, isConfidential: false })).toBe(expected)
  })

  it.each([
    ['EstimatedQuarter', 'Q1 2027 (estimated)'],
    ['EstimatedMonth', 'March 2027 (estimated)'],
    ['ActualDate', '12 March 2027'],
  ] as const)('marks confidential %s dates', (datePrecision, expected) => {
    expect(
      formatRegulatoryDate({
        dateValue: '2027-03-12',
        datePrecision,
        isConfidential: true,
      }),
    ).toBe(`${expected} (confidential)`)
  })
})
