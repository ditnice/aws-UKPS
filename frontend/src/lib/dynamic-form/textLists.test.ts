import { describe, expect, it } from 'vitest'

import type { RecordPageQuestionDto } from '@/client/generated'

import { buildPayload, questionError, repeatIndexes, toFormValue } from './answers'
import { itemErrorKey } from './applyServerErrors'

const names: RecordPageQuestionDto = {
  id: 'record_product_detail.generic_names',
  type: 'TextList',
  multiple: true,
  label: 'Active substance',
  hint: null,
  display: null,
  placeholder: null,
  childLabel: null,
  childPlaceholder: null,
  addAnotherLabel: 'Add another active substance',
  dateName: null,
  showWhen: null,
  rules: [
    { kind: 'Required', value: null, message: 'Enter generic name' },
    { kind: 'Distinct', value: null, message: 'Generic names must be distinct' },
  ],
  options: null,
}

describe('text lists', () => {
  it('shows one empty box when there are no entries', () => {
    expect(toFormValue(names, [])).toEqual([''])
  })

  it('keeps the saved entries in order', () => {
    expect(toFormValue(names, ['velpatasvir', 'sofosbuvir'])).toEqual(['velpatasvir', 'sofosbuvir'])
  })

  it('sends the entries in order without blank ones', () => {
    const shown = new Set([names.id])

    expect(buildPayload([names], { q0: ['B', ' ', 'A', ''] }, shown)).toEqual({
      [names.id]: ['B', 'A'],
    })
  })

  it('reports each repeat against its own entry, ignoring case and blanks', () => {
    expect(questionError(names, ['a1', '', 'A1 ', 'b'])).toEqual({
      message: '',
      items: { '2': 'Generic names must be distinct' },
    })
  })

  it('reports a required list with only blank entries against the list', () => {
    expect(questionError(names, ['', ' '])).toEqual({ message: 'Enter generic name', items: {} })
  })

  it('reports each entry that is too long against itself, ahead of a repeat', () => {
    const limited: RecordPageQuestionDto = {
      ...names,
      rules: [...names.rules, { kind: 'MaxLength', value: 3, message: 'Too long' }],
    }

    expect(questionError(limited, ['abcd', '', ' abc ', 'ABCD'])).toEqual({
      message: '',
      items: { '0': 'Too long', '3': 'Too long' },
    })
  })

  it('finds repeats in order', () => {
    expect(repeatIndexes(['a', null, 'A', 'b', 'a'])).toEqual([2, 4])
  })
})

describe('itemErrorKey', () => {
  it('renumbers a server entry key as shown, skipping blank entries', () => {
    expect(itemErrorKey(names, ['', 'X', '', 'Y'], `${names.id}[1]`)).toBe('3')
  })

  it('ignores the list key itself and other questions', () => {
    expect(itemErrorKey(names, ['X'], names.id)).toBeUndefined()
    expect(itemErrorKey(names, ['X'], 'record_product_detail.other_identifiers[0]')).toBeUndefined()
  })
})
