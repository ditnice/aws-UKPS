import type { ValidationProblemDetails } from '@/client/generated'
import { updateFormApiErrors } from '@/lib/form/formErrorHandling'

import { fieldName, isTextList } from './answers'

import type { FormValue, Question } from './types'
import type { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'

type FieldMetaSetter = {
  setFieldMeta: (field: string, updater: Updater<AnyFieldLikeMetaBase>) => void
  getFieldValue: (field: string) => unknown
}

/**
 * The position of a text list entry as shown, from its position on the server, which drops blank
 * entries before numbering them.
 */
const shownIndex = (entries: unknown, serverIndex: number): number => {
  const filled = (Array.isArray(entries) ? entries : [])
    .map((entry, index) => ({ entry, index }))
    .filter(({ entry }) => typeof entry === 'string' && entry.trim() !== '')
  return filled[serverIndex]?.index ?? serverIndex
}

/**
 * Converts the server's key for an error on a text list entry, such as
 * `record_product_detail.generic_names[1]`, to its key in `QuestionError.items`, renumbered as
 * shown. Undefined for any other key.
 */
export const itemErrorKey = (
  question: Question,
  value: FormValue | undefined,
  key: string,
): string | undefined => {
  const rest = key.startsWith(`${question.id}[`) ? key.slice(question.id.length) : undefined
  if (rest === undefined) {
    return undefined
  }
  if (isTextList(question)) {
    const match = /^\[(\d+)\]$/.exec(rest)
    return match ? String(shownIndex(value, Number(match[1]))) : undefined
  }
  return undefined
}

/**
 * Shows server validation errors against their fields; errors on a text list's entries go on its
 * field as item errors. Returns the messages for keys that belong to no question on the page, so
 * they can be shown as a general error.
 */
export const applyServerErrors = (
  formApi: FieldMetaSetter,
  questions: Question[],
  problem: ValidationProblemDetails,
): string[] => {
  const handled = new Set<string>()
  questions.forEach((question, index) => {
    const name = fieldName(index)
    if (!isTextList(question)) {
      formApi.setFieldMeta(name, updateFormApiErrors(problem, question.id))
      handled.add(question.id)
      return
    }
    const value = formApi.getFieldValue(name) as FormValue | undefined
    const items: Record<string, string> = {}
    for (const [key, messages] of Object.entries(problem.errors)) {
      const itemKey = itemErrorKey(question, value, key)
      if (itemKey !== undefined && messages[0]) {
        items[itemKey] = messages[0]
        handled.add(key)
      }
    }
    const message = problem.errors[question.id]?.[0]
    handled.add(question.id)
    formApi.setFieldMeta(name, (meta) => ({
      ...meta,
      errorMap: {
        ...meta.errorMap,
        onSubmit:
          message || Object.keys(items).length > 0
            ? { message: message ?? '', params: { items } }
            : undefined,
      },
    }))
  })

  return Object.entries(problem.errors)
    .filter(([key]) => !handled.has(key))
    .flatMap(([, messages]) => messages)
}
