import type { RecordAnswersOptionDto, RecordAnswersQuestionDto } from '@/client/generated'

import { isDateControl } from './dates'

import type { DateControl } from './dates'

const notProvidedText = 'Not yet provided'

const formatList = (values: readonly string[]): string =>
  values.length > 0 ? values.join(', ') : notProvidedText

const parseDateOnly = (value: string): Date => {
  const [year, month, day] = value.split('-').map(Number)
  return new Date(Date.UTC(year, month - 1, day))
}

const monthYearFormat = new Intl.DateTimeFormat('en-GB', {
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
})

const fullDateFormat = new Intl.DateTimeFormat('en-GB', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
})

const quarterPattern = /^(\d{4})-Q([1-4])$/

const formatDateAnswer = (answer: string, control: DateControl): string => {
  const quarter = quarterPattern.exec(answer)
  if (quarter) {
    return `Q${quarter[2]} ${quarter[1]} (estimated)`
  }
  if (answer.split('-').length === 3) {
    return fullDateFormat.format(parseDateOnly(answer))
  }
  const month = monthYearFormat.format(parseDateOnly(`${answer}-01`))
  return control === 'ActualMonth' ? month : `${month} (estimated)`
}

const optionLabel = ({ label, parentLabel }: RecordAnswersOptionDto) =>
  parentLabel ? `${parentLabel}: ${label}` : label

/** The text a summary row shows for a question's saved answer. */
export const formatAnswer = ({
  type,
  values,
  selectedOptions,
}: RecordAnswersQuestionDto): string => {
  if (selectedOptions) {
    return formatList(selectedOptions.map(optionLabel))
  }
  if (values.length === 0) {
    return notProvidedText
  }
  return isDateControl(type) ? formatDateAnswer(values[0], type) : formatList(values)
}
