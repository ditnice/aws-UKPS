import {
  formatList,
  fullDateFormat,
  monthYearFormat,
  notProvidedText,
  parseDateOnly,
} from '@/app/portal/organisations/[id]/records/[recordId]/_lib/recordValueFormatters'
import type { RecordAnswersOptionDto, RecordAnswersQuestionDto } from '@/client/generated'

import { isDateControl } from './dates'

import type { DateControl } from './dates'

const quarterPattern = /^(\d{4})-Q([1-4])$/

// The wording mirrors formatRegulatoryDate (recordValueFormatters.ts); keep the two in step
// until ViewRecord moves onto check your answers.
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

/** The text a check your answers row shows for a question's saved answer. */
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
