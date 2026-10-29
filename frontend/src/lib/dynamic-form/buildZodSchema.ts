import z from 'zod'

import { fieldName, validateQuestion } from './answers'

import type { FormValue, Question } from './types'

/**
 * A schema that runs each question's rules on its form value, with the same messages and order
 * as the server.
 */
export const buildZodSchema = (questions: Question[]) =>
  z.object(
    Object.fromEntries(
      questions.map((question, index) => [
        fieldName(index),
        z.custom<FormValue>().superRefine((value, ctx) => {
          const message = validateQuestion(question, value)
          if (message) {
            ctx.addIssue({ code: 'custom', message })
          }
        }),
      ]),
    ),
  )
