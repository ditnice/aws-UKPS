import { describe, expect, it, vi } from 'vitest'

import { applyServerErrors } from './applyServerErrors'

describe('applyServerErrors', () => {
  it('sets field errors by question ID and returns errors for other keys', () => {
    const setFieldMeta = vi.fn()

    const unmatched = applyServerErrors(
      { setFieldMeta },
      {
        errors: {
          'medicines_product_detail.indication': ['Enter the indication'],
          unknown_question: ['This is not a question on this page.'],
        },
      },
      ['medicines_product_detail.indication'],
    )

    expect(setFieldMeta).toHaveBeenCalledTimes(1)
    expect(setFieldMeta.mock.calls[0]?.[0]).toBe('medicines_product_detail.indication')
    const updater = setFieldMeta.mock.calls[0]?.[1] as (meta: object) => {
      errorMap: { onSubmit: string[] }
    }
    expect(updater({ errorMap: {} }).errorMap.onSubmit).toEqual(['Enter the indication'])
    expect(unmatched).toEqual(['This is not a question on this page.'])
  })
})
