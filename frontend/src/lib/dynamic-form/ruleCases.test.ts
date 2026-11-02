import { readFileSync } from 'node:fs'
import path from 'node:path'

import { describe, expect, it } from 'vitest'

import { firstFailure, normaliseAnswer } from './answers'

import type { Answer, Rule } from './types'

type RuleCase = {
  name: string
  multiple: boolean
  rules: Rule[]
  answer: Answer
  error: string | null
}

// Shared with the backend's RuleCasesTests so the two rule implementations cannot drift.
const fixturePath = path.resolve(
  __dirname,
  '../../../../backend/tests/Application/RecordForms/Fixtures/rule-cases.json',
)
const { cases } = JSON.parse(readFileSync(fixturePath, 'utf8')) as { cases: RuleCase[] }

describe('shared rule cases', () => {
  it.each(cases.map((c) => [c.name, c] as const))('%s', (_, ruleCase) => {
    expect(firstFailure(ruleCase.rules, normaliseAnswer(ruleCase.answer)) ?? null).toBe(
      ruleCase.error,
    )
  })
})
