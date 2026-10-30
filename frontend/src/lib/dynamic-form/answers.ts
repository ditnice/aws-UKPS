import { RuleKind } from '@/client/generated'

import type {
  Answer,
  FormValue,
  FormValues,
  HierarchicalEntry,
  Option,
  Question,
  Rule,
} from './types'

/** Server message for a multi-value answer that repeats a value; mirrors RecordFormMessages.Duplicate. */
export const duplicateMessage = 'You have selected the same option more than once.'

const emptyEntry: HierarchicalEntry = { parent: '', child: '' }

/**
 * The TanStack Form field name of the question at `index`. Question IDs contain dots, which
 * TanStack Form would read as nested paths, so fields are named by position instead.
 */
export const fieldName = (index: number) => `q${index}`

const isHierarchical = (question: Question) => question.type === 'HierarchicalSelect'

/** Finds which parent option holds `value`, as a parent and child pair. */
export const findEntry = (options: Option[] | null, value: string): HierarchicalEntry => {
  for (const option of options ?? []) {
    if (option.value === value) {
      return { parent: value, child: '' }
    }
    if (option.children?.some((child) => child.value === value)) {
      return { parent: option.value, child: value }
    }
  }
  return emptyEntry
}

/** Converts a saved answer into the value the form edits. */
export const toFormValue = (question: Question, answer: unknown): FormValue => {
  if (question.multiple) {
    const values = Array.isArray(answer)
      ? answer.filter((v): v is string => typeof v === 'string')
      : []
    if (!isHierarchical(question)) {
      return values
    }
    const entries = values.map((value) => findEntry(question.options, value))
    return entries.length > 0 ? entries : [emptyEntry]
  }

  const value = typeof answer === 'string' ? answer : ''
  return isHierarchical(question) ? findEntry(question.options, value) : value
}

export const toFormValues = (questions: Question[], answers: Record<string, unknown>): FormValues =>
  Object.fromEntries(questions.map((q, i) => [fieldName(i), toFormValue(q, answers[q.id])]))

const entryValue = (entry: HierarchicalEntry) => entry.child || entry.parent

/** Converts a form value into its answer on the wire. */
export const toAnswer = (question: Question, value: FormValue | undefined): Answer => {
  if (question.multiple) {
    if (!Array.isArray(value)) {
      return []
    }
    return value
      .map((item) => (typeof item === 'string' ? item : entryValue(item)))
      .filter((item) => item !== '')
  }

  if (value && typeof value === 'object' && !Array.isArray(value)) {
    return entryValue(value) || null
  }
  return typeof value === 'string' && value !== '' ? value : null
}

/**
 * Normalises an answer the way the server does before running rules: text is trimmed and
 * whitespace-only becomes null; list values are trimmed.
 */
export const normaliseAnswer = (answer: Answer): Answer => {
  if (Array.isArray(answer)) {
    return answer.map((value) => value.trim())
  }
  const trimmed = answer?.trim()
  return trimmed ? trimmed : null
}

const isEmpty = (answer: Answer) => (Array.isArray(answer) ? answer.length === 0 : answer === null)

export const isSatisfied = (rule: Rule, answer: Answer): boolean => {
  switch (rule.kind) {
    case RuleKind.REQUIRED:
      return !isEmpty(answer)
    case RuleKind.MAX_LENGTH:
      return typeof answer !== 'string' || answer.length <= (rule.value ?? Infinity)
    case RuleKind.MAX_ITEMS:
      return !Array.isArray(answer) || answer.length <= (rule.value ?? Infinity)
    default:
      throw new Error(`Unsupported rule kind ${String(rule.kind)}`)
  }
}

const ruleOrder: Rule['kind'][] = [RuleKind.REQUIRED, RuleKind.MAX_LENGTH, RuleKind.MAX_ITEMS]

/** The message of the first failing rule, in the same fixed order as the server, or undefined. */
export const firstFailure = (rules: Rule[], answer: Answer): string | undefined =>
  [...rules]
    .sort((a, b) => ruleOrder.indexOf(a.kind) - ruleOrder.indexOf(b.kind))
    .find((rule) => !isSatisfied(rule, answer))?.message

/** Validates one question's form value; returns the message to show, or undefined. */
export const validateQuestion = (question: Question, value: FormValue | undefined) => {
  const answer = normaliseAnswer(toAnswer(question, value))
  if (Array.isArray(answer) && new Set(answer).size !== answer.length) {
    return duplicateMessage
  }
  return firstFailure(question.rules, answer)
}

/** Builds the save payload: every question on the page, keyed by question ID. */
export const buildPayload = (questions: Question[], values: FormValues): Record<string, Answer> =>
  Object.fromEntries(questions.map((q, i) => [q.id, toAnswer(q, values[fieldName(i)])]))
