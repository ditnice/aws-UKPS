import { readFileSync } from 'node:fs'
import path from 'node:path'

import { describe, expect, it } from 'vitest'

import { dateAnswerError, precisionOf } from './dates'

type DateCase = {
  name: string
  answer: string
  error: string | null
  precision?: string
}

// Shared with the backend's DateCasesTests so the two date parsers cannot drift.
const fixturePath = path.resolve(
  __dirname,
  '../../../../backend/tests/Application/RecordForms/Fixtures/date-cases.json',
)
const { today, cases } = JSON.parse(readFileSync(fixturePath, 'utf8')) as {
  today: string
  cases: DateCase[]
}

describe('shared date cases', () => {
  it.each(cases.map((c) => [c.name, c] as const))('%s', (_, dateCase) => {
    expect(dateAnswerError(dateCase.answer, today)).toBe(dateCase.error)
    expect(precisionOf(dateCase.answer)).toBe(dateCase.precision ?? null)
  })
})
