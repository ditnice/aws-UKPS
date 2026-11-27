import { AnyFieldLikeMetaBase } from '@tanstack/react-form'
import { describe, expect, it } from 'vitest'

import { clearFormApiErrors, getFormStateServerErrors } from './formErrorHandling'

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

describe('getFormStateServerErrors', () => {
  const errors = {
    ConfirmationCode: ['The confirmation code is incorrect'],
    Email: ['Enter a valid email address'],
  }

  it('puts each error on its field, keyed in lower camel case', () => {
    expect(getFormStateServerErrors(errors).errorMap.onServer.fields).toEqual({
      confirmationCode: ['The confirmation code is incorrect'],
      email: ['Enter a valid email address'],
    })
  })

  it('returns every error as a form-level error when no field names are given', () => {
    expect(getFormStateServerErrors(errors).errors).toEqual([
      'The confirmation code is incorrect',
      'Enter a valid email address',
    ])
  })

  it('leaves the errors of the named fields out of the form-level errors', () => {
    expect(getFormStateServerErrors(errors, ['confirmationCode']).errors).toEqual([
      'Enter a valid email address',
    ])
  })
})
