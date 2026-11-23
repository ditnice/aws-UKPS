import { describe, expect, it } from 'vitest'

import type { RecordPageQuestionDto } from '@/client/generated'

import { questionError, repeatIndexes, toFormValue, toFormValues, toPayloadAnswer } from './answers'
import { itemErrorKey } from './applyServerErrors'

const base: RecordPageQuestionDto = {
  id: 'q',
  type: 'Text',
  multiple: false,
  label: 'Q',
  hint: null,
  display: null,
  placeholder: null,
  childLabel: null,
  childPlaceholder: null,
  addAnotherLabel: null,
  dateName: null,
  showWhen: null,
  rules: [],
  options: null,
}

const numbers: RecordPageQuestionDto = {
  ...base,
  id: 'numbers',
  type: 'TextList',
  multiple: true,
  rules: [{ kind: 'Distinct', value: null, message: 'Repeated number' }],
}

const group: RecordPageQuestionDto = {
  ...base,
  id: 'record_clinical_trial',
  type: 'Group',
  rules: [{ kind: 'Required', value: null, message: 'Add at least one' }],
  children: [
    { ...base, id: 'name', rules: [{ kind: 'Required', value: null, message: 'Enter a name' }] },
    {
      ...base,
      id: 'code',
      type: 'PrefixedNumber',
      prefixes: ['NCT'],
      prefixDigits: 8,
      numberName: 'Trial number',
      rules: [{ kind: 'Distinct', value: null, message: 'Repeated code' }],
    },
    numbers,
  ],
}

describe('text lists', () => {
  it('shows one empty box when there are no entries', () => {
    expect(toFormValue(numbers, [])).toEqual([''])
  })

  it('sends the entries in order without blank ones', () => {
    expect(toPayloadAnswer(numbers, ['B', ' ', 'A', ''])).toEqual(['B', 'A'])
  })

  it('reports each repeat against its own entry, ignoring case and blanks', () => {
    expect(questionError(numbers, ['a1', '', 'A1 ', 'b'])).toEqual({
      message: '',
      items: { '2': 'Repeated number' },
    })
  })
})

describe('groups', () => {
  it('shows one blank item when there are none', () => {
    expect(toFormValue(group, [])).toEqual([{ name: '', code: '', numbers: [''] }])
  })

  it('adds a blank item after the saved ones when asked to', () => {
    const saved = { name: 'A', code: 'NCT12345678', numbers: ['X'] }

    expect(toFormValues([group], { record_clinical_trial: [saved] }, true)).toEqual({
      q0: [saved, { name: '', code: '', numbers: [''] }],
    })
  })

  it('shows only the one blank item when asked to add one and nothing is saved', () => {
    expect(toFormValues([group], { record_clinical_trial: [] }, true)).toEqual({
      q0: [{ name: '', code: '', numbers: [''] }],
    })
  })

  it('reads and sends each item keyed by child ID', () => {
    const answer = [{ name: 'A', code: 'NCT12345678', numbers: ['X'] }]

    expect(toPayloadAnswer(group, toFormValue(group, answer))).toEqual(answer)
  })

  it('reports the group rule and each item problem under its key', () => {
    expect(questionError(group, [])).toEqual({ message: 'Add at least one', items: {} })
    expect(
      questionError(group, [
        { name: '', code: 'NCT12345678', numbers: ['x', 'X'] },
        { name: 'B', code: 'NCT12345678', numbers: [''] },
        { name: 'C', code: 'NCT1', numbers: [''] },
      ]),
    ).toEqual({
      message: '',
      items: {
        '0.name': 'Enter a name',
        '0.numbers.1': 'Repeated number',
        '1.code': 'Repeated code',
        '2.code': 'Trial number must be 8 digits long',
      },
    })
  })

  it('finds repeats in order', () => {
    expect(repeatIndexes(['a', null, 'A', 'b', 'a'])).toEqual([2, 4])
  })
})

describe('itemErrorKey', () => {
  it('maps a group child key and renumbers text list entries as shown', () => {
    const value = [{ name: 'A', code: '', numbers: ['', 'X', '', 'Y'] }]

    expect(itemErrorKey(group, value, 'record_clinical_trial[0].name')).toBe('0.name')
    expect(itemErrorKey(group, value, 'record_clinical_trial[0].numbers[1]')).toBe('0.numbers.3')
    expect(itemErrorKey(group, value, 'record_clinical_trial')).toBeUndefined()
    expect(itemErrorKey(group, value, 'record_clinical_trial_information.x')).toBeUndefined()
  })

  it('maps a top-level text list entry', () => {
    expect(itemErrorKey(numbers, ['', 'X'], 'numbers[0]')).toBe('1')
  })
})
