import type { FormQuestionDto } from '@/client/generated'

/** Props every question component receives from `FormPage`. */
export type QuestionProps = {
  question: FormQuestionDto
  value: unknown
  onChange: (value: string | string[]) => void
  onBlur: () => void
  error?: string
  disabled: boolean
  /** The label repeats the page heading, so it is visually hidden. */
  labelAsHeading: boolean
}

export const hintId = (question: FormQuestionDto) => `${question.id}-hint`
