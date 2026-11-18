import { describe, expect, it } from 'vitest'

import type { RecordCheckAnswersQuestionDto } from '@/client/generated'

import { formatAnswer } from './answerDisplay'

const question = (
  overrides: Partial<RecordCheckAnswersQuestionDto>,
): RecordCheckAnswersQuestionDto => ({
  id: 'section.task.question',
  pageId: 'page',
  label: 'Question',
  type: 'Text',
  multiple: false,
  values: [],
  selectedOptions: null,
  itemLabel: null,
  items: null,
  ...overrides,
})

describe('formatAnswer', () => {
  it('shows a text answer as saved', () => {
    expect(formatAnswer(question({ values: ['Aspirin'] }))).toBe('Aspirin')
  })

  it('shows "Not yet provided" when there is no answer', () => {
    expect(formatAnswer(question({}))).toBe('Not yet provided')
  })

  it('lists the labels of the chosen options', () => {
    const selectedOptions = [
      { value: 'a', label: 'Option A', parentLabel: null },
      { value: 'b', label: 'Option B', parentLabel: null },
    ]

    expect(
      formatAnswer(
        question({ type: 'Checkbox', multiple: true, values: ['a', 'b'], selectedOptions }),
      ),
    ).toBe('Option A, Option B')
  })

  it('puts the parent label before a hierarchical option', () => {
    const selectedOptions = [{ value: 'c', label: 'Child', parentLabel: 'Parent' }]

    expect(
      formatAnswer(question({ type: 'HierarchicalSelect', values: ['c'], selectedOptions })),
    ).toBe('Parent: Child')
  })

  it('shows "Not yet provided" when a choice has nothing selected', () => {
    expect(formatAnswer(question({ type: 'Radio', selectedOptions: [] }))).toBe('Not yet provided')
  })

  it.each([
    ['PrecisionDate', '2027-Q2', 'Q2 2027 (estimated)'],
    ['PrecisionDate', '2027-06', 'June 2027 (estimated)'],
    ['PrecisionDate', '2027-06-14', '14 June 2027'],
    ['ActualMonth', '2027-06', 'June 2027'],
    ['ActualDate', '2027-06-14', '14 June 2027'],
  ] as const)('formats a %s answer of %s', (type, value, expected) => {
    expect(formatAnswer(question({ type, values: [value] }))).toBe(expected)
  })
})
