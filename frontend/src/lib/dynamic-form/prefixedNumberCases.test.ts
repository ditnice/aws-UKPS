import { readFileSync } from 'node:fs'
import path from 'node:path'

import { describe, expect, it } from 'vitest'

import { normaliseAnswer } from './answers'
import {
  evaluatePrefixedNumber,
  prefixedNumberErrorParts,
  splitPrefixedNumber,
} from './prefixedNumber'

type PrefixedNumberCase = {
  name: string
  prefixes: string[]
  digits?: number
  numberName?: string
  answer: string | null
  value: string | null
  error: string | null
}

// Shared with the backend's PrefixedNumberCasesTests so the two implementations cannot drift.
const fixturePath = path.resolve(
  __dirname,
  '../../../../backend/tests/Application/RecordForms/Fixtures/prefixed-number-cases.json',
)
const { cases } = JSON.parse(readFileSync(fixturePath, 'utf8')) as { cases: PrefixedNumberCase[] }

describe('shared prefixed number cases', () => {
  it.each(cases.map((c) => [c.name, c] as const))('%s', (_, prefixedCase) => {
    const answer = normaliseAnswer(prefixedCase.answer) as string | null

    expect(
      evaluatePrefixedNumber(
        prefixedCase.prefixes,
        answer,
        prefixedCase.digits,
        prefixedCase.numberName,
      ),
    ).toEqual({
      value: prefixedCase.value,
      error: prefixedCase.error,
    })
  })
})

describe('splitPrefixedNumber', () => {
  const prefixes = ['GID-TA', 'GID-HST']

  it.each([
    ['', '', ''],
    ['GID-TA', 'GID-TA', ''],
    ['GID-HST123', 'GID-HST', '123'],
    ['123', '', '123'],
    ['gid-ta1', '', 'gid-ta1'],
  ])('splits %j into prefix %j and number %j', (value, prefix, number) => {
    expect(splitPrefixedNumber(prefixes, value)).toEqual({ prefix, number })
  })
})

describe('prefixedNumberErrorParts', () => {
  it.each([
    ['Select ID type', undefined, { prefix: true, number: false }],
    ['ID must be a whole positive number', undefined, { prefix: false, number: true }],
    ['ID must not be longer than 5 digits', undefined, { prefix: false, number: true }],
    [
      'Trial number must be a whole positive number',
      'Trial number',
      { prefix: false, number: true },
    ],
    ['Trial number must be 8 digits long', 'Trial number', { prefix: false, number: true }],
    ['Trial number must be 8 digits long', undefined, { prefix: true, number: true }],
    ['Enter the trial number', 'Trial number', { prefix: true, number: true }],
  ])('puts %j (named %j) on %j', (error, name, parts) => {
    expect(prefixedNumberErrorParts(error, name)).toEqual(parts)
  })

  it('marks neither box without an error', () => {
    expect(prefixedNumberErrorParts(undefined, 'Trial number')).toEqual({
      prefix: false,
      number: false,
    })
  })
})
