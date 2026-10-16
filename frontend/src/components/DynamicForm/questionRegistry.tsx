import { QuestionType } from '@/client/generated'
import type { FormQuestionDto } from '@/client/generated'

import { CheckboxQuestion } from './questions/CheckboxQuestion'
import { ComboboxSlot } from './questions/ComboboxSlot'
import { RadioQuestion } from './questions/RadioQuestion'
import { SelectQuestion } from './questions/SelectQuestion'
import { TextareaQuestion } from './questions/TextareaQuestion'

import type { QuestionProps } from './questions/types'
import type { ComponentType } from 'react'

const components: Record<QuestionType, ComponentType<QuestionProps>> = {
  [QuestionType.TEXTAREA]: TextareaQuestion,
  [QuestionType.SELECT]: SelectQuestion,
  [QuestionType.RADIO]: RadioQuestion,
  [QuestionType.CHECKBOX]: CheckboxQuestion,
}

/** The component that renders a question, honouring its `display` hint. */
export const getQuestionComponent = (question: FormQuestionDto): ComponentType<QuestionProps> =>
  question.type === QuestionType.CHECKBOX && question.display === 'combobox'
    ? ComboboxSlot
    : components[question.type]

/**
 * The element the error summary links to: the control itself, or the first option of a radio
 * or checkbox group (NDS gives each option the ID `{name}_{value}`).
 */
export const getQuestionTargetId = (question: FormQuestionDto): string => {
  const firstOption = question.options?.[0]
  return (question.type === QuestionType.RADIO || question.type === QuestionType.CHECKBOX) &&
    firstOption
    ? `${question.id}_${firstOption.value}`
    : question.id
}
