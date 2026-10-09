import { describe, expect, it } from 'vitest'

import type { FormQuestionDto } from '@/client/generated'

import { buildPayload, getPath, normaliseAnswer, toFormValues } from './answers'

const questions: Pick<FormQuestionDto, 'id' | 'type'>[] = [
  { id: 'medicines_product_detail.indication', type: 'Textarea' },
  { id: 'medicines_product_detail.indication_is_cancer', type: 'Radio' },
  { id: 'medicines_product_detail_therapeutic_area', type: 'Checkbox' },
]

describe('toFormValues', () => {
  it('nests values by the dots in question IDs and fills blanks', () => {
    expect(
      toFormValues(questions, {
        'medicines_product_detail.indication': 'Hepatitis C',
        'medicines_product_detail.indication_is_cancer': null,
        medicines_product_detail_therapeutic_area: ['4'],
      }),
    ).toEqual({
      medicines_product_detail: { indication: 'Hepatitis C', indication_is_cancer: '' },
      medicines_product_detail_therapeutic_area: ['4'],
    })
  })

  it('treats missing answers as unanswered', () => {
    expect(toFormValues(questions, {})).toEqual({
      medicines_product_detail: { indication: '', indication_is_cancer: '' },
      medicines_product_detail_therapeutic_area: [],
    })
  })
})

describe('buildPayload', () => {
  it('flattens to question IDs, trims text and sends blanks as null or []', () => {
    const values = {
      medicines_product_detail: { indication: '  Hepatitis C  ', indication_is_cancer: '' },
      medicines_product_detail_therapeutic_area: [],
    }

    expect(buildPayload(questions, values)).toEqual({
      'medicines_product_detail.indication': 'Hepatitis C',
      'medicines_product_detail.indication_is_cancer': null,
      medicines_product_detail_therapeutic_area: [],
    })
  })

  it('includes every question even if its value is missing', () => {
    expect(Object.keys(buildPayload(questions, {}))).toEqual(questions.map((q) => q.id))
  })
})

describe('normaliseAnswer', () => {
  it('does not trim option values', () => {
    expect(normaliseAnswer('Radio', ' Yes ')).toBe(' Yes ')
  })

  it('turns whitespace-only text into null', () => {
    expect(normaliseAnswer('Textarea', ' \n ')).toBeNull()
  })
})

describe('getPath', () => {
  it('reads a nested value', () => {
    expect(getPath({ a: { b: 'c' } }, 'a.b')).toBe('c')
  })

  it('returns undefined for a missing path', () => {
    expect(getPath({}, 'a.b')).toBeUndefined()
  })
})
