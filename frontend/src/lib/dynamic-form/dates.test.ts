import { afterEach, describe, expect, it, vi } from 'vitest'

import { toDateAnswer, toDateValue, utcToday } from './dates'

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

  it('reads an actual date only as a date', () => {
    expect(toDateValue('2025-07-26', 'ActualDate')).toEqual({
      ...empty,
      precision: 'ActualDate',
      day: '26',
      month: '7',
      year: '2025',
    })
    expect(toDateValue('2025-07', 'ActualDate')).toEqual({ ...empty, precision: 'ActualDate' })
    expect(toDateValue(null, 'ActualDate')).toEqual({ ...empty, precision: 'ActualDate' })
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
    expect(toDateAnswer({ ...empty, precision: 'ActualDate' })).toBeNull()
  })

  it('leaves an incomplete date for validation to reject', () => {
    expect(toDateAnswer({ ...empty, precision: 'ActualDate', month: '4', year: '2027' })).toBe(
      '2027-04-',
    )
    expect(toDateAnswer({ ...empty, precision: 'EstimatedQuarter', year: '27' })).toBe('27-Q')
  })
})
