import { ConditionKind } from '@/client/generated'

import { fieldName, normaliseAnswer, toAnswer } from './answers'

import type { Answer, Condition, FormValues, Question } from './types'

/**
 * Whether a condition holds. `answerOf` returns a trigger's answer, which is unanswered when the
 * trigger itself is hidden. Mirrors the server's FormCondition; the shared fixture
 * condition-cases.json keeps the two in step.
 */
export const isConditionMet = (
  condition: Condition,
  answerOf: (questionId: string) => Answer,
): boolean => {
  switch (condition.kind) {
    case ConditionKind.ANSWER_IN: {
      const answer = answerOf(condition.questionId)
      const values = Array.isArray(answer) ? answer : answer === null ? [] : [answer]
      return values.some((value) => condition.values.includes(value))
    }
    default:
      throw new Error(`Unsupported condition kind ${String(condition.kind)}`)
  }
}

/**
 * The IDs of the page's questions that apply to the current form values. A trigger always comes
 * before the questions it reveals, so one pass in page order lets chains of conditions cascade.
 */
export const shownQuestionIds = (questions: Question[], values: FormValues): Set<string> => {
  const indexById = new Map(questions.map((q, i) => [q.id, i]))
  const shown = new Set<string>()
  const answerOf = (questionId: string): Answer => {
    const index = indexById.get(questionId)
    return index !== undefined && shown.has(questionId)
      ? normaliseAnswer(toAnswer(questions[index], values[fieldName(index)]))
      : null
  }

  for (const question of questions) {
    if (!question.showWhen || isConditionMet(question.showWhen, answerOf)) {
      shown.add(question.id)
    }
  }
  return shown
}

/**
 * Where each question is rendered. A question revealed by a single option of an earlier Radio is
 * nested beneath that option (`nested`, keyed by trigger index then option value); every other
 * question is rendered at the top level of the page (`topLevel`, in page order).
 */
export const pageLayout = (questions: Question[]) => {
  const topLevel: number[] = []
  const nested = new Map<number, Map<string, number[]>>()

  questions.forEach((question, index) => {
    const condition = question.showWhen
    const triggerIndex = condition ? questions.findIndex((q) => q.id === condition.questionId) : -1
    const trigger = questions[triggerIndex]
    if (!condition || !trigger || condition.values.length !== 1 || trigger.type !== 'Radio') {
      topLevel.push(index)
      return
    }

    const byOption = nested.get(triggerIndex) ?? new Map<string, number[]>()
    const value = condition.values[0]
    byOption.set(value, [...(byOption.get(value) ?? []), index])
    nested.set(triggerIndex, byOption)
  })

  return { topLevel, nested }
}
