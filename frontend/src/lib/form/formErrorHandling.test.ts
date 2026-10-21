import { AnyFieldLikeMetaBase } from '@tanstack/react-form'
import { describe, expect, it } from 'vitest'

import { clearFormApiErrors } from './formErrorHandling'

const applyUpdater = (meta: Partial<AnyFieldLikeMetaBase>) => {
  const updater = clearFormApiErrors()
  if (typeof updater !== 'function') {
    throw new Error('Expected clearFormApiErrors to return an updater function')
  }
  return updater(meta as AnyFieldLikeMetaBase)
}

describe('clearFormApiErrors', () => {
  it('clears the submit error', () => {
    expect(applyUpdater({ errorMap: { onSubmit: 'API error' } }).errorMap.onSubmit).toBeUndefined()
  })

  it('keeps errors from other validators', () => {
    expect(
      applyUpdater({ errorMap: { onSubmit: 'API error', onDynamic: 'Enter a value' } }).errorMap,
    ).toEqual({ onSubmit: undefined, onDynamic: 'Enter a value' })
  })

  it('keeps the rest of the field meta', () => {
    expect(
      applyUpdater({ errorMap: { onSubmit: 'API error' }, isTouched: true, isBlurred: true }),
    ).toMatchObject({ isTouched: true, isBlurred: true })
  })
})
