import { z } from 'zod'

import type { FormQuestionDto } from '@/client/generated'

import { firstRuleError, normaliseAnswer } from './answers'

type SchemaShape = { [key: string]: z.ZodType | SchemaShape }

const questionSchema = (question: FormQuestionDto) =>
  z.unknown().superRefine((value, ctx) => {
    const message = firstRuleError(question.rules, normaliseAnswer(question.type, value))
    if (message) {
      ctx.addIssue({ code: 'custom', message })
    }
  })

const toObjectSchema = (shape: SchemaShape): z.ZodObject =>
  z.object(
    Object.fromEntries(
      Object.entries(shape).map(([key, value]) => [
        key,
        value instanceof z.ZodType ? value : toObjectSchema(value),
      ]),
    ),
  )

/**
 * Builds a Zod schema for a page that mirrors the server's rules, nested by the dots in
 * question IDs to match the form values (see `toFormValues`). Each question reports at most
 * one error: the first failing rule's message, as defined on the server.
 */
export const buildZodSchema = (questions: FormQuestionDto[]): z.ZodObject => {
  const shape: SchemaShape = {}
  for (const question of questions) {
    const keys = question.id.split('.')
    const last = keys.pop() as string
    let current = shape
    for (const key of keys) {
      current[key] ??= {}
      current = current[key] as SchemaShape
    }
    current[last] = questionSchema(question)
  }
  return toObjectSchema(shape)
}
