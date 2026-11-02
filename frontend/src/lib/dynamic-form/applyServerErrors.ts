import type { ValidationProblemDetails } from '@/client/generated'
import { updateFormApiErrors } from '@/lib/form/formErrorHandling'

import { fieldName } from './answers'

import type { Question } from './types'
import type { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'

type FieldMetaSetter = {
  setFieldMeta: (field: string, updater: Updater<AnyFieldLikeMetaBase>) => void
}

/**
 * Shows server validation errors against their fields. Returns the messages for keys that are
 * not questions on the page, so they can be shown as a general error.
 */
export const applyServerErrors = (
  formApi: FieldMetaSetter,
  questions: Question[],
  problem: ValidationProblemDetails,
): string[] => {
  questions.forEach((question, index) => {
    formApi.setFieldMeta(fieldName(index), updateFormApiErrors(problem, question.id))
  })

  const questionIds = new Set(questions.map((q) => q.id))
  return Object.entries(problem.errors)
    .filter(([key]) => !questionIds.has(key))
    .flatMap(([, messages]) => messages)
}
