import z from 'zod'

import { fieldName, questionError } from './answers'
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
        const error = questionError(question, values[fieldName(index)])
        if (error) {
          // The parts let a date question highlight only the inputs at fault.
          ctx.addIssue({
            code: 'custom',
            message: error.message,
            path: [fieldName(index)],
            params: error.parts && { parts: error.parts },
          })
        }
      })
    })
