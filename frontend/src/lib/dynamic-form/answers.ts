import { FormControl, RuleKind } from '@/client/generated'

import {
  dateAnswerError,
  isDateControl,
  precisionDateProblem,
  toDateAnswer,
  toDateValue,
  utcToday,
} from './dates'
import { evaluatePrefixedNumber } from './prefixedNumber'

import type { DatePart } from './dates'
import type {
  Answer,
  FormValue,
  FormValues,
  GroupItemAnswer,
  GroupItemValue,
  HierarchicalEntry,
  Option,
  PayloadAnswer,
  Question,
  Rule,
} from './types'

/** Server message for a multi-value answer that repeats a value; mirrors RecordFormMessages.Duplicate. */
export const duplicateMessage = 'You have selected the same option more than once'

const emptyEntry: HierarchicalEntry = { parent: '', child: '' }

/**
 * The TanStack Form field name of the question at `index`. Question IDs contain dots, which
 * TanStack Form would read as nested paths, so fields are named by position instead.
 */
export const fieldName = (index: number) => `q${index}`

const isHierarchical = (question: Question) => question.type === FormControl.HIERARCHICAL_SELECT

export const isGroup = (question: Question) => question.type === FormControl.GROUP

export const isTextList = (question: Question) => question.type === FormControl.TEXT_LIST

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null && !Array.isArray(value)

/** A group's items, from its form value. */
export const groupItems = (value: FormValue | undefined): GroupItemValue[] =>
  (Array.isArray(value) ? (value as unknown[]) : []).filter(isRecord) as GroupItemValue[]

/** A new, blank item of a group: each child's unanswered form value. */
export const emptyGroupItem = (question: Question): GroupItemValue =>
  Object.fromEntries(
    (question.children ?? []).map((child) => [
      child.id,
      toFormValue(child, null) as string | string[],
    ]),
  )

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
  if (isDateControl(question.type)) {
    return toDateValue(answer, question.type)
  }
  if (isGroup(question)) {
    // A group always shows at least one item to fill in.
    const items = (Array.isArray(answer) ? answer : [])
      .filter(isRecord)
      .map((item): GroupItemValue =>
        Object.fromEntries(
          (question.children ?? []).map((child) => [
            child.id,
            toFormValue(child, item[child.id]) as string | string[],
          ]),
        ),
      )
    return items.length > 0 ? items : [emptyGroupItem(question)]
  }
  if (question.multiple) {
    const values = Array.isArray(answer)
      ? answer.filter((v): v is string => typeof v === 'string')
      : []
    if (isTextList(question)) {
      // A text list always shows at least one box to type in.
      return values.length > 0 ? values : ['']
    }
    if (!isHierarchical(question)) {
      return values
    }
    const entries = values.map((value) => findEntry(question.options, value))
    return entries.length > 0 ? entries : [emptyEntry]
  }

  const value = typeof answer === 'string' ? answer : ''
  return isHierarchical(question) ? findEntry(question.options, value) : value
}

export const toFormValues = (
  questions: Question[],
  answers: Record<string, unknown>,
  addGroupItem = false,
): FormValues =>
  Object.fromEntries(
    questions.map((q, i) => {
      const value = toFormValue(q, answers[q.id])
      const saved = answers[q.id]
      // A group with nothing saved already shows one blank item, so only add after saved items.
      const addBlank = addGroupItem && isGroup(q) && Array.isArray(saved) && saved.length > 0
      return [fieldName(i), addBlank ? [...groupItems(value), emptyGroupItem(q)] : value]
    }),
  )

const entryValue = (entry: HierarchicalEntry) => entry.child || entry.parent

/** Converts a form value into its answer on the wire. */
export const toAnswer = (question: Question, value: FormValue | undefined): Answer => {
  if (isDateControl(question.type)) {
    return value && typeof value === 'object' && 'precision' in value ? toDateAnswer(value) : null
  }
  if (question.multiple) {
    if (!Array.isArray(value)) {
      return []
    }
    return value
      .map((item) =>
        typeof item === 'string'
          ? item
          : 'parent' in item
            ? entryValue(item as HierarchicalEntry)
            : '',
      )
      .filter((item) => item.trim() !== '')
  }

  if (value && typeof value === 'object' && 'parent' in value) {
    return entryValue(value) || null
  }
  return typeof value === 'string' && value !== '' ? value : null
}

/** Converts a group's form value into its answer on the wire: one object per item. */
export const toGroupAnswer = (
  question: Question,
  value: FormValue | undefined,
): GroupItemAnswer[] =>
  groupItems(value).map((item) =>
    Object.fromEntries(
      (question.children ?? []).map((child) => [child.id, toAnswer(child, item[child.id])]),
    ),
  )

/** Converts any question's form value into its answer on the wire, including a group's. */
export const toPayloadAnswer = (question: Question, value: FormValue | undefined): PayloadAnswer =>
  isGroup(question) ? toGroupAnswer(question, value) : toAnswer(question, value)

/**
 * Normalises an answer the way the server does before running rules: text is trimmed and
 * whitespace-only becomes null; list values are trimmed and blank ones dropped.
 */
export const normaliseAnswer = (answer: Answer): Answer => {
  if (Array.isArray(answer)) {
    return answer.map((value) => value.trim()).filter((value) => value !== '')
  }
  const trimmed = answer?.trim()
  return trimmed ? trimmed : null
}

const isEmpty = (answer: Answer) => (Array.isArray(answer) ? answer.length === 0 : answer === null)

/** Whether a trimmed value, or one text list entry, is within a MaxLength rule. */
const isShortEnough = (rule: Rule, value: string | null) =>
  value === null || value.length <= (rule.value ?? Infinity)

export const isSatisfied = (rule: Rule, answer: Answer): boolean => {
  switch (rule.kind) {
    case RuleKind.REQUIRED:
      return !isEmpty(answer)
    case RuleKind.MAX_LENGTH:
      return [answer].flat().every((value) => isShortEnough(rule, value))
    case RuleKind.MAX_ITEMS:
      return !Array.isArray(answer) || answer.length <= (rule.value ?? Infinity)
    case RuleKind.DISTINCT:
      return !Array.isArray(answer) || repeatIndexes(answer).length === 0
    default:
      throw new Error(`Unsupported rule kind ${String(rule.kind)}`)
  }
}

const ruleOrder: Rule['kind'][] = [
  RuleKind.REQUIRED,
  RuleKind.MAX_LENGTH,
  RuleKind.MAX_ITEMS,
  RuleKind.DISTINCT,
]

/**
 * The positions of values that repeat an earlier one, ignoring case and surrounding space.
 * Blank values are never repeats. Mirrors RuleEvaluator.RepeatIndexes, which compares with
 * `OrdinalIgnoreCase`: that upper-cases each character, so values are upper-cased here too.
 */
export const repeatIndexes = (values: (string | null)[]): number[] => {
  const seen = new Set<string>()
  return values.flatMap((value, index) => {
    const key = value?.trim().toUpperCase()
    if (!key) {
      return []
    }
    if (seen.has(key)) {
      return [index]
    }
    seen.add(key)
    return []
  })
}

const distinctMessage = (question: Question) =>
  question.rules.find((rule) => rule.kind === RuleKind.DISTINCT)?.message

/** The rules other than Distinct, which is reported against each repeat rather than the question. */
const ownRules = (question: Question) =>
  question.rules.filter((rule) => rule.kind !== RuleKind.DISTINCT)

/** The message of the first failing rule, in the same fixed order as the server, or undefined. */
export const firstFailure = (rules: Rule[], answer: Answer): string | undefined =>
  [...rules]
    .sort((a, b) => ruleOrder.indexOf(a.kind) - ruleOrder.indexOf(b.kind))
    .find((rule) => !isSatisfied(rule, answer))?.message

/**
 * An error on a question: its message, the parts of a date it is about when only some are at
 * fault, and the errors of a text list's entries or a group's items. Item errors are keyed by
 * position and child ID joined with dots, as in `1` (a text list entry), `0.study_name` (a group
 * item's question) or `0.other_registry_numbers.1` (an entry of a text list in a group item). The
 * message is empty when only items are at fault.
 */
export type QuestionError = { message: string; parts?: DatePart[]; items?: Record<string, string> }

/** The errors of a text list's entries, keyed by position, and of the list as a whole. */
const textListError = (question: Question, value: FormValue | undefined) => {
  const entries = Array.isArray(value)
    ? value.filter((v): v is string => typeof v === 'string')
    : []
  const items: Record<string, string> = {}
  const message = distinctMessage(question)
  if (message) {
    for (const index of repeatIndexes(entries)) {
      items[String(index)] = message
    }
  }
  // Reported in place of a repeat, as MaxLength comes before Distinct.
  const maxLength = question.rules.find((rule) => rule.kind === RuleKind.MAX_LENGTH)
  if (maxLength) {
    entries.forEach((entry, index) => {
      if (!isShortEnough(maxLength, entry.trim())) {
        items[String(index)] = maxLength.message
      }
    })
  }
  const listRules = ownRules(question).filter((rule) => rule.kind !== RuleKind.MAX_LENGTH)
  return { message: firstFailure(listRules, normaliseAnswer(entries)), items }
}

/** The value of a group child that must differ between items, as the server compares it. */
const comparableValue = (question: Question, value: FormValue | undefined) => {
  const answer = normaliseAnswer(toAnswer(question, value))
  if (typeof answer !== 'string') {
    return null
  }
  return question.prefixes
    ? evaluatePrefixedNumber(question.prefixes, answer, question.prefixDigits, question.numberName)
        .value
    : answer
}

/**
 * The errors of a group: its own rules (such as at least one item), then each item's questions,
 * then each answer that must differ between items but repeats an earlier one.
 */
const groupError = (
  question: Question,
  value: FormValue | undefined,
): QuestionError | undefined => {
  const items = groupItems(value)
  const errors: Record<string, string> = {}
  for (const child of question.children ?? []) {
    items.forEach((item, index) => {
      const error = questionError(child, item[child.id])
      if (error?.message) {
        errors[`${index}.${child.id}`] = error.message
      }
      for (const [key, message] of Object.entries(error?.items ?? {})) {
        errors[`${index}.${child.id}.${key}`] = message
      }
    })
    const message = !isTextList(child) && distinctMessage(child)
    if (message) {
      for (const index of repeatIndexes(
        items.map((item) => comparableValue(child, item[child.id])),
      )) {
        errors[`${index}.${child.id}`] ??= message
      }
    }
  }
  // The group's own rules count items, so any array of the same length stands in for them.
  const message = firstFailure(
    ownRules(question),
    items.map(() => 'item'),
  )
  return message || Object.keys(errors).length > 0
    ? { message: message ?? '', items: errors }
    : undefined
}

/** Validates one question's form value; returns the error to show, or undefined. */
export const questionError = (
  question: Question,
  value: FormValue | undefined,
): QuestionError | undefined => {
  if (isGroup(question)) {
    return groupError(question, value)
  }
  if (isTextList(question)) {
    const { message, items } = textListError(question, value)
    return message || Object.keys(items).length > 0 ? { message: message ?? '', items } : undefined
  }
  if (
    question.type === 'PrecisionDate' &&
    value &&
    typeof value === 'object' &&
    'precision' in value
  ) {
    const problem = precisionDateProblem(value, question.dateName)
    if (problem) {
      return problem
    }
  }
  const answer = normaliseAnswer(toAnswer(question, value))
  if (Array.isArray(answer) && new Set(answer).size !== answer.length) {
    return { message: duplicateMessage }
  }
  if (question.prefixes && !Array.isArray(answer)) {
    // The server saves a prefix with no digits as blank before running the rules.
    const { value: saved, error } = evaluatePrefixedNumber(
      question.prefixes,
      answer,
      question.prefixDigits,
      question.numberName,
    )
    const prefixMessage = firstFailure(question.rules, saved) ?? error
    return prefixMessage ? { message: prefixMessage } : undefined
  }
  const message =
    (isDateControl(question.type) && typeof answer === 'string'
      ? dateAnswerError(answer, question.type, utcToday(), question.dateName)
      : null) ?? firstFailure(question.rules, answer)
  return message ? { message } : undefined
}

/** Validates one question's form value; returns the message to show, or undefined. */
export const validateQuestion = (question: Question, value: FormValue | undefined) =>
  questionError(question, value)?.message

/** The answer of an unanswered question: null, or an empty array when multi-value or a group. */
const emptyAnswer = (question: Question): PayloadAnswer =>
  question.multiple || isGroup(question) ? [] : null

/**
 * Builds the save payload: every question on the page, keyed by question ID. Questions not in
 * `shown` (hidden by their conditions) are sent unanswered.
 */
export const buildPayload = (
  questions: Question[],
  values: FormValues,
  shown: ReadonlySet<string>,
): Record<string, PayloadAnswer> =>
  Object.fromEntries(
    questions.map((q, i) => [
      q.id,
      shown.has(q.id) ? toPayloadAnswer(q, values[fieldName(i)]) : emptyAnswer(q),
    ]),
  )
