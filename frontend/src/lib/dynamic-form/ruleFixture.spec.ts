// @vitest-environment node
import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'

import { describe, expect, it } from 'vitest'

import type { FormQuestionDto, FormRuleDto, QuestionType } from '@/client/generated'

import { firstRuleError, normaliseAnswer } from './answers'
import { buildZodSchema } from './buildZodSchema'

/**
 * The shared rule fixture is also run by the backend (RuleFixtureTests), so the client and
 * server agree on every rule kind.
 */
type RuleCase = {
  name: string
  type: QuestionType
  rules: FormRuleDto[]
  value: string | string[] | null
  error: string | null
}

const fixturePath = fileURLToPath(
  new URL('../../../../backend/tests/Application/Forms/Fixtures/rule-cases.json', import.meta.url),
)
const { cases } = JSON.parse(readFileSync(fixturePath, 'utf8')) as { cases: RuleCase[] }

const toQuestion = (testCase: RuleCase): FormQuestionDto => ({
  id: 'table.field',
  type: testCase.type,
  label: 'Label',
  rules: testCase.rules,
})

describe('shared rule fixture', () => {
  it('has cases', () => {
    expect(cases.length).toBeGreaterThan(0)
  })

  it.each(cases.map((testCase) => [testCase.name, testCase] as const))(
    '%s: firstRuleError',
    (_name, testCase) => {
      const error = firstRuleError(testCase.rules, normaliseAnswer(testCase.type, testCase.value))

      expect(error ?? null).toBe(testCase.error)
    },
  )

  it.each(cases.map((testCase) => [testCase.name, testCase] as const))(
    '%s: buildZodSchema',
    (_name, testCase) => {
      // Form values use '' for an unanswered single-value question.
      const formValue = testCase.value ?? ''
      const result = buildZodSchema([toQuestion(testCase)]).safeParse({
        table: { field: formValue },
      })

      const messages = result.success ? [] : result.error.issues.map((issue) => issue.message)
      expect(messages).toEqual(testCase.error ? [testCase.error] : [])
      if (!result.success) {
        expect(result.error.issues[0]?.path).toEqual(['table', 'field'])
      }
    },
  )
})
