import type { FormControl } from '@/client/generated'

import type { Answer } from './types'

// Server messages; mirror RecordFormMessages.
export const invalidDateMessage = 'Enter a real date.'
export const actualDateInFutureMessage = 'An actual date must be today or in the past.'
export const estimatedDateInPastMessage = 'An estimated date must be in the future.'
export const actualMonthInFutureMessage = 'The date must be this month or in the past.'

export type DatePrecision = 'EstimatedQuarter' | 'EstimatedMonth' | 'ActualDate' | 'ActualMonth'

/** The controls whose answer is a date: a date at a chosen precision, or an actual month. */
export type DateControl = Extract<FormControl, 'PrecisionDate' | 'ActualMonth'>

export const isDateControl = (control: FormControl): control is DateControl =>
  control === 'PrecisionDate' || control === 'ActualMonth'

/**
 * A date question's value while the form is being edited: the chosen precision ('' until one is
 * chosen; always ActualMonth for that control) and the text typed into each part.
 */
export type DateValue = {
  precision: DatePrecision | ''
  day: string
  month: string
  quarter: string
  year: string
}

// Mirrors DateAnswer: a PrecisionDate is yyyy-Qn, yyyy-MM or yyyy-MM-dd, and an ActualMonth is
// yyyy-MM, with a four-digit year from 1000. An actual date is today or earlier, an actual month
// this month or earlier, and an estimate is for a quarter or month that has not ended.
const pattern =
  /^(?<year>[1-9][0-9]{3})-(?:Q(?<quarter>[1-4])|(?<month>0[1-9]|1[0-2])(?:-(?<day>0[1-9]|[12][0-9]|3[01]))?)$/

type ParsedDate = {
  precision: DatePrecision
  year: number
  quarter?: number
  month?: number
  day?: number
  /** The last day of the quarter or month, or the actual date, as yyyy-MM-dd. */
  lastDay: string
}

const daysInMonth = (year: number, month: number) => new Date(Date.UTC(year, month, 0)).getUTCDate()

const isoDate = (year: number, month: number, day: number) =>
  `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`

const endOfMonth = (year: number, month: number) => isoDate(year, month, daysInMonth(year, month))

/** Today's date as yyyy-MM-dd, in the user's time zone. */
export const localToday = () => {
  const now = new Date()
  return isoDate(now.getFullYear(), now.getMonth() + 1, now.getDate())
}

/** Parses a date answer, or returns null when it is not a real date in one of the control's formats. */
const parseDateAnswer = (answer: string, control: DateControl): ParsedDate | null => {
  const groups = pattern.exec(answer)?.groups
  const isMonth = Boolean(groups?.month) && !groups?.day
  if (!groups || (control === 'ActualMonth' && !isMonth)) {
    return null
  }
  const year = Number(groups.year)
  if (groups.quarter) {
    const quarter = Number(groups.quarter)
    return { precision: 'EstimatedQuarter', year, quarter, lastDay: endOfMonth(year, quarter * 3) }
  }
  const month = Number(groups.month)
  if (isMonth) {
    const precision = control === 'ActualMonth' ? 'ActualMonth' : 'EstimatedMonth'
    return { precision, year, month, lastDay: endOfMonth(year, month) }
  }
  const day = Number(groups.day)
  return day <= daysInMonth(year, month)
    ? { precision: 'ActualDate', year, month, day, lastDay: isoDate(year, month, day) }
    : null
}

/** The error for a date answer, or null when it is a real date on the right side of today. */
export const dateAnswerError = (
  answer: string,
  control: DateControl = 'PrecisionDate',
  today: string = localToday(),
): string | null => {
  const parsed = parseDateAnswer(answer, control)
  if (!parsed) {
    return invalidDateMessage
  }
  if (parsed.precision === 'ActualDate') {
    return parsed.lastDay > today ? actualDateInFutureMessage : null
  }
  if (parsed.precision === 'ActualMonth') {
    return isoDate(parsed.year, parsed.month ?? 1, 1) > today ? actualMonthInFutureMessage : null
  }
  return parsed.lastDay < today ? estimatedDateInPastMessage : null
}

/**
 * The precision of a PrecisionDate answer, which is what conditions on a date compare; null when
 * unanswered or not real.
 */
export const precisionOf = (answer: Answer): DatePrecision | null =>
  typeof answer === 'string' ? (parseDateAnswer(answer, 'PrecisionDate')?.precision ?? null) : null

const emptyParts = { day: '', month: '', quarter: '', year: '' }

/** Converts a saved date answer into the value the form edits. */
export const toDateValue = (answer: unknown, control: DateControl = 'PrecisionDate'): DateValue => {
  const parsed = typeof answer === 'string' ? parseDateAnswer(answer, control) : null
  if (!parsed) {
    return { precision: control === 'ActualMonth' ? 'ActualMonth' : '', ...emptyParts }
  }
  return {
    precision: parsed.precision,
    day: parsed.day?.toString() ?? '',
    month: parsed.month?.toString() ?? '',
    quarter: parsed.quarter?.toString() ?? '',
    year: parsed.year.toString(),
  }
}

const partsOf: Record<DatePrecision, (keyof typeof emptyParts)[]> = {
  EstimatedQuarter: ['quarter', 'year'],
  EstimatedMonth: ['month', 'year'],
  ActualDate: ['day', 'month', 'year'],
  ActualMonth: ['month', 'year'],
}

const pad = (part: string) => (/^\d$/.test(part) ? `0${part}` : part)

/**
 * Converts a date value into its answer on the wire. A precision with no parts entered is
 * unanswered. Parts are not checked here, so an incomplete or impossible date gives an answer that
 * `dateAnswerError` rejects.
 */
export const toDateAnswer = ({ precision, ...parts }: DateValue): string | null => {
  if (!precision || partsOf[precision].every((part) => parts[part].trim() === '')) {
    return null
  }
  const [day, month, quarter, year] = [parts.day, parts.month, parts.quarter, parts.year].map(
    (part) => part.trim(),
  )
  switch (precision) {
    case 'EstimatedQuarter':
      return `${year}-Q${quarter}`
    case 'EstimatedMonth':
    case 'ActualMonth':
      return `${year}-${pad(month)}`
    case 'ActualDate':
      return `${year}-${pad(month)}-${pad(day)}`
  }
}
