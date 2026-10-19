import { describe, expect, it } from 'vitest'

import {
  buildPayload,
  duplicateMessage,
  fieldName,
  findEntry,
  isSatisfied,
  toAnswer,
  toFormValue,
  toFormValues,
  validateQuestion,
} from './answers'
import { applyServerErrors } from './applyServerErrors'
import { buildZodSchema } from './buildZodSchema'

import type { Option, Question } from './types'

const tree: Option[] = [
  {
    value: '1',
    label: '1: Addiction',
    children: [
      { value: '11', label: '1.1: Opioid use disorder', children: null },
      { value: '12', label: '1.2: NAS', children: null },
    ],
  },
  { value: '2', label: '2: Allergy', children: null },
]

const question = (overrides: Partial<Question>): Question => ({
  id: 'entity.field',
  type: 'Text',
  multiple: false,
  label: 'Label',
  hint: null,
  display: null,
  placeholder: null,
  childLabel: null,
  childPlaceholder: null,
  addAnotherLabel: null,
  rules: [],
  options: null,
  ...overrides,
})

const text = question({
  rules: [{ kind: 'Required', value: null, message: 'Enter a value' }],
})
const bnf = question({ id: 'bnf', type: 'HierarchicalSelect', options: tree })
const areas = question({
  id: 'areas',
  type: 'HierarchicalSelect',
  multiple: true,
  options: tree,
  rules: [{ kind: 'MaxItems', value: 2, message: 'Select up to 2' }],
})
const ticks = question({ id: 'ticks', type: 'Checkbox', multiple: true, options: tree })

describe('findEntry', () => {
  it('finds parents, children and unknown values', () => {
    expect(findEntry(tree, '1')).toEqual({ parent: '1', child: '' })
    expect(findEntry(tree, '12')).toEqual({ parent: '1', child: '12' })
    expect(findEntry(tree, '99')).toEqual({ parent: '', child: '' })
    expect(findEntry(null, '1')).toEqual({ parent: '', child: '' })
  })
})

describe('toFormValue and toAnswer', () => {
  it('round-trips text, treating null as empty', () => {
    expect(toFormValue(text, null)).toBe('')
    expect(toFormValue(text, 'abc')).toBe('abc')
    expect(toAnswer(text, '')).toBeNull()
    expect(toAnswer(text, 'abc')).toBe('abc')
  })

  it('round-trips a hierarchical single value as its most specific choice', () => {
    expect(toFormValue(bnf, '11')).toEqual({ parent: '1', child: '11' })
    expect(toAnswer(bnf, { parent: '1', child: '11' })).toBe('11')
    expect(toAnswer(bnf, { parent: '2', child: '' })).toBe('2')
    expect(toAnswer(bnf, { parent: '', child: '' })).toBeNull()
  })

  it('round-trips hierarchical multi values, dropping empty entries', () => {
    expect(toFormValue(areas, [])).toEqual([{ parent: '', child: '' }])
    expect(toFormValue(areas, ['2', '12'])).toEqual([
      { parent: '2', child: '' },
      { parent: '1', child: '12' },
    ])
    expect(
      toAnswer(areas, [
        { parent: '1', child: '' },
        { parent: '', child: '' },
      ]),
    ).toEqual(['1'])
  })

  it('round-trips checkboxes', () => {
    expect(toFormValue(ticks, ['1', 3])).toEqual(['1'])
    expect(toFormValue(ticks, 'oops')).toEqual([])
    expect(toAnswer(ticks, ['1', '2'])).toEqual(['1', '2'])
    expect(toAnswer(ticks, 'oops')).toEqual([])
  })
})

describe('validateQuestion', () => {
  it('runs rules on the normalised answer', () => {
    expect(validateQuestion(text, '   ')).toBe('Enter a value')
    expect(validateQuestion(text, ' a ')).toBeUndefined()
  })

  it('reports duplicates and too many entries', () => {
    expect(
      validateQuestion(areas, [
        { parent: '2', child: '' },
        { parent: '2', child: '' },
      ]),
    ).toBe(duplicateMessage)
    expect(
      validateQuestion(areas, [
        { parent: '2', child: '' },
        { parent: '1', child: '' },
        { parent: '1', child: '11' },
      ]),
    ).toBe('Select up to 2')
  })

  it('rejects unknown rule kinds', () => {
    expect(() =>
      isSatisfied({ kind: 'Unknown' as never, value: null, message: '' }, null),
    ).toThrow()
  })
})

describe('form values and payload', () => {
  it('names fields by position and builds a payload keyed by question ID', () => {
    const questions = [text, bnf]
    const values = toFormValues(questions, { [text.id]: 'abc', bnf: '11' })

    expect(Object.keys(values)).toEqual([fieldName(0), fieldName(1)])
    expect(buildPayload(questions, values)).toEqual({ [text.id]: 'abc', bnf: '11' })
  })
})

describe('buildZodSchema', () => {
  it('reports rule failures against field names', () => {
    const result = buildZodSchema([text]).safeParse({ q0: '' })

    expect(result.success).toBe(false)
    expect(result.error?.issues[0]).toMatchObject({ path: ['q0'], message: 'Enter a value' })
    expect(buildZodSchema([text]).safeParse({ q0: 'a' }).success).toBe(true)
  })
})

describe('applyServerErrors', () => {
  it('sets field errors and returns messages for keys not on the page', () => {
    const calls: string[] = []
    const others = applyServerErrors({ setFieldMeta: (field) => calls.push(field) }, [text], {
      errors: { [text.id]: ['Bad'], stray: ['Not on page'] },
    })

    expect(calls).toEqual(['q0'])
    expect(others).toEqual(['Not on page'])
  })
})
