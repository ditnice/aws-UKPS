import { readFileSync } from 'node:fs'
import path from 'node:path'

import { describe, expect, it } from 'vitest'

import { normaliseAnswer } from './answers'
import { isConditionMet } from './conditions'

import type { Answer, Condition } from './types'

type ConditionCase = {
  name: string
  condition: Condition
  answer: Answer
  met: boolean
}

// Shared with the backend's ConditionCasesTests so the two condition implementations cannot drift.
const fixturePath = path.resolve(
  __dirname,
  '../../../../backend/tests/Application/RecordForms/Fixtures/condition-cases.json',
)
const { cases } = JSON.parse(readFileSync(fixturePath, 'utf8')) as { cases: ConditionCase[] }

describe('shared condition cases', () => {
  it.each(cases.map((c) => [c.name, c] as const))('%s', (_, conditionCase) => {
    expect(
      isConditionMet(conditionCase.condition, () => normaliseAnswer(conditionCase.answer)),
    ).toBe(conditionCase.met)
  })
})
