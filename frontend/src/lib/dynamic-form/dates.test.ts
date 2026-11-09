import { afterEach, describe, expect, it, vi } from 'vitest'

import { localToday, toDateAnswer, toDateValue } from './dates'

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
})

describe('localToday', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('is the date in the user’s time zone, padded', () => {
    vi.useFakeTimers({ toFake: ['Date'] })
    vi.setSystemTime(new Date(2026, 2, 5, 23, 30))
    expect(localToday()).toBe('2026-03-05')
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
  })

  it('is unanswered with no precision, or a precision with nothing entered', () => {
    expect(toDateAnswer({ ...empty, year: '2027' })).toBeNull()
    expect(toDateAnswer({ ...empty, precision: 'ActualDate', quarter: '2' })).toBeNull()
  })

  it('leaves an incomplete date for validation to reject', () => {
    expect(toDateAnswer({ ...empty, precision: 'ActualDate', month: '4', year: '2027' })).toBe(
      '2027-04-',
    )
    expect(toDateAnswer({ ...empty, precision: 'EstimatedQuarter', year: '27' })).toBe('27-Q')
  })
})
