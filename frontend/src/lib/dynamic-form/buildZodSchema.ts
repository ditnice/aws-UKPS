import z from 'zod'

import { fieldName, validateQuestion } from './answers'
import { shownQuestionIds } from './conditions'

import type { FormValue, Question } from './types'

/**
 * A schema that runs each shown question's rules on its form value, with the same messages and
 * order as the server. Questions hidden by their conditions are not validated.
 */
export const buildZodSchema = (questions: Question[]) =>
  z
    .object(
      Object.fromEntries(questions.map((_, index) => [fieldName(index), z.custom<FormValue>()])),
    )
    .superRefine((values, ctx) => {
      const shown = shownQuestionIds(questions, values)
      questions.forEach((question, index) => {
        if (!shown.has(question.id)) {
          return
        }
        const message = validateQuestion(question, values[fieldName(index)])
        if (message) {
          ctx.addIssue({ code: 'custom', message, path: [fieldName(index)] })
        }
      })
    })
