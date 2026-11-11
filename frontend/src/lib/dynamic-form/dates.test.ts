import { afterEach, describe, expect, it, vi } from 'vitest'

import { precisionDateProblem, toDateAnswer, toDateValue, utcToday } from './dates'

import type { DateValue } from './dates'

const empty: DateValue = { precision: '', day: '', month: '', quarter: '', year: '' }

describe('toDateValue', () => {
  it('splits each precision into its parts', () => {
    expect(toDateValue('2027-Q2')).toEqual({
      ...empty,
      precision: 'EstimatedQuarter',
      quarter: '2',
      year: '2027',
    })
    expect(toDateValue('2027-04')).toEqual({
      ...empty,
      precision: 'EstimatedMonth',
      month: '4',
      year: '2027',
    })
    expect(toDateValue('2027-04-05')).toEqual({
      precision: 'ActualDate',
      day: '5',
      month: '4',
      quarter: '',
      year: '2027',
    })
  })

  it('starts an unanswered date on no precision', () => {
    expect(toDateValue(null)).toEqual(empty)
  })

  it('reads an actual month only as a month', () => {
    expect(toDateValue('2025-07', 'ActualMonth')).toEqual({
      ...empty,
      precision: 'ActualMonth',
      month: '7',
      year: '2025',
    })
    expect(toDateValue('2025-07-26', 'ActualMonth')).toEqual({ ...empty, precision: 'ActualMonth' })
    expect(toDateValue(null, 'ActualMonth')).toEqual({ ...empty, precision: 'ActualMonth' })
  })
})

describe('utcToday', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('is the date in UTC, padded', () => {
    vi.useFakeTimers({ toFake: ['Date'] })
    vi.setSystemTime(new Date(Date.UTC(2026, 2, 5, 23, 30)))
    expect(utcToday()).toBe('2026-03-05')
  })

  it('is the UTC date when British Summer Time is already on the next day', () => {
    vi.useFakeTimers({ toFake: ['Date'] })
    vi.setSystemTime(new Date(Date.UTC(2026, 5, 15, 23, 30)))
    expect(utcToday()).toBe('2026-06-15')
  })
})

describe('toDateAnswer', () => {
  it('pads the month and day and keeps only the chosen precision’s parts', () => {
    const value: DateValue = {
      precision: 'ActualDate',
      day: '5',
      month: '4',
      quarter: '3',
      year: '2027',
    }
    expect(toDateAnswer(value)).toBe('2027-04-05')
    expect(toDateAnswer({ ...value, precision: 'EstimatedMonth' })).toBe('2027-04')
    expect(toDateAnswer({ ...value, precision: 'EstimatedQuarter' })).toBe('2027-Q3')
    expect(toDateAnswer({ ...value, precision: 'ActualMonth' })).toBe('2027-04')
  })

  it('is unanswered with no precision, or a precision with nothing entered', () => {
    expect(toDateAnswer({ ...empty, year: '2027' })).toBeNull()
    expect(toDateAnswer({ ...empty, precision: 'ActualDate', quarter: '2' })).toBeNull()
    expect(toDateAnswer({ ...empty, precision: 'ActualMonth' })).toBeNull()
  })

  it('leaves an incomplete date for validation to reject', () => {
    expect(toDateAnswer({ ...empty, precision: 'ActualDate', month: '4', year: '2027' })).toBe(
      '2027-04-',
    )
    expect(toDateAnswer({ ...empty, precision: 'EstimatedQuarter', year: '27' })).toBe('27-Q')
  })
})

describe('precisionDateProblem', () => {
  const today = '2026-06-15'
  const problem = (value: Partial<DateValue>) =>
    precisionDateProblem({ ...empty, ...value }, 'submission', today)

  it('has no problem until a precision is chosen, or with a real date on the right side', () => {
    expect(problem({})).toBeNull()
    expect(problem({ precision: 'EstimatedQuarter', quarter: '3', year: '2026' })).toBeNull()
    expect(problem({ precision: 'ActualDate', day: '15', month: '06', year: '2026' })).toBeNull()
    expect(problem({ precision: 'ActualDate', day: ' 29 ', month: '2', year: '2024' })).toBeNull()
  })

  it('asks for the date when a part is blank, about the blank parts only', () => {
    expect(problem({ precision: 'ActualDate', month: '13', year: ' ' })).toEqual({
      message: 'Enter the actual submission date',
      parts: ['day', 'year'],
    })
    expect(problem({ precision: 'EstimatedQuarter', year: '2027' })).toEqual({
      message: 'Enter the estimated submission date',
      parts: ['quarter'],
    })
  })

  it('rejects parts that are not real, about those parts only', () => {
    const notReal = (value: Partial<DateValue>) => problem(value)?.parts
    expect(notReal({ precision: 'EstimatedMonth', month: 'AB', year: '2027' })).toEqual(['month'])
    expect(notReal({ precision: 'EstimatedMonth', month: '0', year: '2027' })).toEqual(['month'])
    expect(notReal({ precision: 'EstimatedMonth', month: '7', year: '20AB' })).toEqual(['year'])
    expect(notReal({ precision: 'EstimatedMonth', month: '7', year: '0999' })).toEqual(['year'])
    expect(notReal({ precision: 'EstimatedMonth', month: '7', year: '27' })).toEqual(['year'])
    expect(notReal({ precision: 'EstimatedQuarter', quarter: '5', year: '2027' })).toEqual([
      'quarter',
    ])
    expect(notReal({ precision: 'ActualDate', day: '32', month: '13', year: '2025' })).toEqual([
      'day',
      'month',
    ])
    expect(problem({ precision: 'ActualDate', day: '29', month: '2', year: '2025' })).toEqual({
      message: 'Actual date of submission must be a real date',
      parts: ['day'],
    })
  })

  it('checks the day against its month only once the month and year are real', () => {
    expect(problem({ precision: 'ActualDate', day: '31', month: '4', year: 'x' })?.parts).toEqual([
      'year',
    ])
  })

  it('rejects a date on the wrong side of today, about every part', () => {
    expect(problem({ precision: 'EstimatedQuarter', quarter: '1', year: '2026' })).toEqual({
      message: 'Estimated date of submission must be in the future',
      parts: ['quarter', 'year'],
    })
    expect(problem({ precision: 'ActualDate', day: '16', month: '6', year: '2026' })).toEqual({
      message: 'Actual date of submission must be in the past',
      parts: ['day', 'month', 'year'],
    })
  })
})
