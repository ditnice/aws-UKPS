import { describe, expect, it } from 'vitest'

import { fieldName } from './answers'
import { isConditionMet, pageLayout, shownQuestionIds } from './conditions'

import type { DateValue } from './dates'
import type { Question } from './types'

const question = (overrides: Partial<Question>): Question => ({
  id: 'q',
  type: 'Radio',
  multiple: false,
  label: 'Label',
  hint: null,
  display: null,
  placeholder: null,
  childLabel: null,
  childPlaceholder: null,
  addAnotherLabel: null,
  dateName: null,
  showWhen: null,
  rules: [],
  options: [
    { value: 'Yes', label: 'Yes', hint: null, children: null },
    { value: 'No', label: 'No', hint: null, children: null },
  ],
  ...overrides,
})

const gate = question({ id: 'gate' })
const detail = question({
  id: 'detail',
  showWhen: { kind: 'AnswerIn', questionId: 'gate', values: ['Yes'] },
})
const notes = question({
  id: 'notes',
  type: 'Text',
  options: null,
  showWhen: { kind: 'AnswerIn', questionId: 'detail', values: ['Yes'] },
})
const other = question({
  id: 'other',
  type: 'Text',
  options: null,
  showWhen: { kind: 'AnswerIn', questionId: 'gate', values: ['Yes', 'No'] },
})

const values = (...answers: string[]) =>
  Object.fromEntries(answers.map((answer, index) => [fieldName(index), answer]))

describe('shownQuestionIds', () => {
  it('shows unconditional questions and those whose condition is met', () => {
    expect([...shownQuestionIds([gate, detail, notes], values('Yes', 'No', ''))]).toEqual([
      'gate',
      'detail',
    ])
  })

  it('hides questions revealed by a hidden trigger', () => {
    expect([...shownQuestionIds([gate, detail, notes], values('No', 'Yes', 'x'))]).toEqual(['gate'])
  })
})

describe('isConditionMet', () => {
  it('rejects unknown condition kinds', () => {
    expect(() =>
      isConditionMet({ kind: 'Unknown' as never, questionId: 'gate', values: [] }, () => null),
    ).toThrow()
  })
})

describe('pageLayout', () => {
  it('nests questions revealed by one option of a radio, and keeps the rest at the top level', () => {
    const { topLevel, nested } = pageLayout([gate, detail, notes])

    expect(topLevel).toEqual([0])
    expect(nested.get(0)).toEqual(new Map([['Yes', [1]]]))
    expect(nested.get(1)).toEqual(new Map([['Yes', [2]]]))
  })

  it('keeps a question at the top level when its trigger is not a radio', () => {
    const text = question({ id: 'gate', type: 'Text', options: null })
    const checkbox = question({ id: 'gate', type: 'Checkbox', multiple: true })

    expect(pageLayout([text, detail]).topLevel).toEqual([0, 1])
    expect(pageLayout([checkbox, detail]).topLevel).toEqual([0, 1])
  })

  it('nests a question revealed by several options of a radio beneath each of them', () => {
    const { topLevel, nested } = pageLayout([gate, other])

    expect(topLevel).toEqual([0])
    expect(nested.get(0)).toEqual(
      new Map([
        ['Yes', [1]],
        ['No', [1]],
      ]),
    )
  })

  it('compares a date trigger by its precision once a real date is entered', () => {
    const date = question({ id: 'date', type: 'PrecisionDate', options: null })
    const global = question({
      id: 'global',
      type: 'Text',
      options: null,
      showWhen: { kind: 'AnswerIn', questionId: 'date', values: ['ActualDate'] },
    })
    const shown = (value: Partial<DateValue>) =>
      shownQuestionIds([date, global], {
        [fieldName(0)]: { precision: '', day: '', month: '', quarter: '', year: '', ...value },
        [fieldName(1)]: '',
      })

    expect(shown({ precision: 'ActualDate', day: '5', month: '4', year: '2027' })).toContain(
      'global',
    )
    expect(shown({ precision: 'EstimatedMonth', month: '4', year: '2027' })).not.toContain('global')
    expect(shown({ precision: 'ActualDate', day: '31', month: '4', year: '2027' })).not.toContain(
      'global',
    )
  })
})
