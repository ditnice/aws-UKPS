import type { FormControl } from '@/client/generated'

import type { Answer } from './types'

// Server messages; mirror RecordFormMessages. A PrecisionDate's messages name its date, for
// example "submission".
export const invalidDateMessage = 'Enter a real date'
export const actualMonthInFutureMessage = 'The date must be this month or in the past'
export const dateNotRealMessage = (actual: boolean, name: string) =>
  `${actual ? 'Actual' : 'Estimated'} date of ${name} must be a real date`
export const actualDateInFutureMessage = (name: string) =>
  `Actual date of ${name} must be in the past`
export const estimatedDateInPastMessage = (name: string) =>
  `Estimated date of ${name} must be in the future`

// Only the page sees a blank part, as the answer it sends is then incomplete.
export const dateBlankMessage = (actual: boolean, name: string) =>
  `Enter the ${actual ? 'actual' : 'estimated'} ${name} date`

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

/** Today's date as yyyy-MM-dd in UTC, whatever the user's time zone; mirrors DateAnswer.Today. */
export const utcToday = () => {
  const now = new Date()
  return isoDate(now.getUTCFullYear(), now.getUTCMonth() + 1, now.getUTCDate())
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

/**
 * The error for a date answer, or null when it is a real date on the right side of today; mirrors
 * DateAnswer.Check. A PrecisionDate's errors name its date (`dateName`); one that is not real is
 * taken as actual when it has a day.
 */
export const dateAnswerError = (
  answer: string,
  control: DateControl,
  today: string,
  dateName: string | null,
): string | null => {
  if (control === 'PrecisionDate' && !dateName?.trim()) {
    throw new Error('A PrecisionDate question needs a dateName for its messages.')
  }
  const name = dateName ?? ''
  const parsed = parseDateAnswer(answer, control)
  if (!parsed) {
    return control === 'PrecisionDate'
      ? dateNotRealMessage(answer.split('-').length === 3, name)
      : invalidDateMessage
  }
  if (parsed.precision === 'ActualDate') {
    return parsed.lastDay > today ? actualDateInFutureMessage(name) : null
  }
  if (parsed.precision === 'ActualMonth') {
    return isoDate(parsed.year, parsed.month ?? 1, 1) > today ? actualMonthInFutureMessage : null
  }
  return parsed.lastDay < today ? estimatedDateInPastMessage(name) : null
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

export type DatePart = keyof typeof emptyParts

/** The parts each precision is entered in, in the order they are shown. */
export const partsOf: Record<DatePrecision, DatePart[]> = {
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

/** An error on a date question and the parts it is about. */
export type DateProblem = { message: string; parts: DatePart[] }

const isRealYear = (text: string) => /^[1-9]\d{3}$/.test(text)

const isWholeUpTo = (text: string, max: number) =>
  /^\d{1,2}$/.test(text) && Number(text) >= 1 && Number(text) <= max

/** Whether one part is real: a day must also be in its month once the month and year are real. */
const isRealPart = (part: DatePart, value: DateValue) => {
  const [day, month, quarter, year] = [value.day, value.month, value.quarter, value.year].map(
    (text) => text.trim(),
  )
  switch (part) {
    case 'year':
      return isRealYear(year)
    case 'quarter':
      return /^[1-4]$/.test(quarter)
    case 'month':
      return isWholeUpTo(month, 12)
    case 'day':
      return (
        isWholeUpTo(day, 31) &&
        (!isWholeUpTo(month, 12) ||
          !isRealYear(year) ||
          Number(day) <= daysInMonth(Number(year), Number(month)))
      )
  }
}

/**
 * The first problem with a PrecisionDate once a precision is chosen, or null: blank parts, then
 * parts that are not real, then a date on the wrong side of today, which is about every part.
 */
export const precisionDateProblem = (
  value: DateValue,
  dateName: string | null,
  today: string = utcToday(),
): DateProblem | null => {
  const { precision } = value
  if (!precision || precision === 'ActualMonth') {
    return null
  }
  const parts = partsOf[precision]
  const actual = precision === 'ActualDate'
  const blank = parts.filter((part) => value[part].trim() === '')
  if (blank.length > 0) {
    return { message: dateBlankMessage(actual, dateName ?? ''), parts: blank }
  }
  const notReal = parts.filter((part) => !isRealPart(part, value))
  if (notReal.length > 0) {
    return { message: dateNotRealMessage(actual, dateName ?? ''), parts: notReal }
  }
  const message = dateAnswerError(toDateAnswer(value) ?? '', 'PrecisionDate', today, dateName)
  return message ? { message, parts } : null
}
