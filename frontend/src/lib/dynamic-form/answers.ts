import { QuestionType } from '@/client/generated'
import type { FormQuestionDto, FormRuleDto } from '@/client/generated'

/** An answer as sent to and from the API. */
export type AnswerValue = string | string[] | null

/**
 * Form values are nested by the dots in question IDs, because TanStack Form treats a dotted
 * field name as a path: `medicines_product_detail.indication` is stored at
 * `values.medicines_product_detail.indication`.
 */
export type FormValues = Record<string, unknown>

type QuestionShape = Pick<FormQuestionDto, 'id' | 'type'>

export const isMultiValue = (type: QuestionType) => type === QuestionType.CHECKBOX

export const getPath = (values: FormValues, path: string): unknown =>
  path
    .split('.')
    .reduce<unknown>(
      (current, key) =>
        current && typeof current === 'object' ? (current as FormValues)[key] : undefined,
      values,
    )

const setPath = (values: FormValues, path: string, value: unknown) => {
  const keys = path.split('.')
  const last = keys.pop() as string
  let current = values
  for (const key of keys) {
    current[key] ??= {}
    current = current[key] as FormValues
  }
  current[last] = value
}

/**
 * Converts an API answer to a form value: `''` for an unanswered single-value question (so
 * inputs stay controlled), an array for multi-value questions.
 */
const toFormValue = (type: QuestionType, answer: unknown): string | string[] => {
  if (isMultiValue(type)) {
    return Array.isArray(answer) ? answer.map(String) : []
  }
  return typeof answer === 'string' ? answer : ''
}

/** Builds TanStack default values from the page's current answers. */
export const toFormValues = (
  questions: QuestionShape[],
  answers: Record<string, unknown>,
): FormValues => {
  const values: FormValues = {}
  for (const question of questions) {
    setPath(values, question.id, toFormValue(question.type, answers[question.id]))
  }
  return values
}

/**
 * Normalises a form value the same way the server does before validating: text is trimmed
 * and blank becomes `null`; multi-value answers are arrays.
 */
export const normaliseAnswer = (type: QuestionType, value: unknown): AnswerValue => {
  if (isMultiValue(type)) {
    return Array.isArray(value) ? value.map(String) : []
  }
  if (typeof value !== 'string') {
    return null
  }
  const text = type === QuestionType.TEXTAREA ? value.trim() : value
  return text === '' ? null : text
}

/** Builds the save payload: every question on the page, keyed by its ID. */
export const buildPayload = (
  questions: QuestionShape[],
  values: FormValues,
): Record<string, AnswerValue> =>
  Object.fromEntries(
    questions.map((question) => [
      question.id,
      normaliseAnswer(question.type, getPath(values, question.id)),
    ]),
  )

const isSatisfied = (rule: FormRuleDto, answer: AnswerValue): boolean => {
  switch (rule.kind) {
    case 'Required':
      return Array.isArray(answer) ? answer.length > 0 : answer !== null && answer.trim() !== ''
    case 'MaxLength':
      // String.length counts UTF-16 code units, matching the server.
      return typeof answer !== 'string' || answer.length <= (rule.value ?? Infinity)
    case 'MaxItems':
      return !Array.isArray(answer) || answer.length <= (rule.value ?? Infinity)
    default:
      // Unknown rule kinds are enforced by the server.
      return true
  }
}

/**
 * The message of the first rule the (normalised) answer fails, or `undefined` if it passes.
 * Mirrors the server, which reports one error per question.
 */
export const firstRuleError = (rules: FormRuleDto[], answer: AnswerValue): string | undefined =>
  rules.find((rule) => !isSatisfied(rule, answer))?.message
