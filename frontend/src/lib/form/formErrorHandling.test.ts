import { AnyFieldLikeMetaBase, AnyFormApi, Updater } from '@tanstack/react-form'
import { describe, expect, it, vi } from 'vitest'

import { clearAllFormApiErrors, clearFormApiErrors, submitForm } from './formErrorHandling'

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

describe('clearAllFormApiErrors', () => {
  const fakeForm = (fieldMeta: Record<string, AnyFieldLikeMetaBase>) =>
    ({
      state: { fieldMeta },
      setFieldMeta: (name: string, updater: Updater<AnyFieldLikeMetaBase>) => {
        if (typeof updater !== 'function') {
          throw new Error('Expected an updater function')
        }
        fieldMeta[name] = updater(fieldMeta[name])
      },
    }) as unknown as AnyFormApi

  it('clears the submit error on every field and keeps errors from other validators', () => {
    const fieldMeta = {
      companyCode: { errorMap: { onSubmit: 'API error' } },
      brandedName: { errorMap: { onSubmit: 'API error', onDynamic: 'Enter a value' } },
    } as unknown as Record<string, AnyFieldLikeMetaBase>

    clearAllFormApiErrors(fakeForm(fieldMeta))

    expect(fieldMeta.companyCode.errorMap.onSubmit).toBeUndefined()
    expect(fieldMeta.brandedName.errorMap).toEqual({
      onSubmit: undefined,
      onDynamic: 'Enter a value',
    })
  })
})

describe('submitForm', () => {
  it('clears server errors, then re-runs the form validation, then submits with the meta', async () => {
    const calls: string[] = []
    const fieldMeta = {
      companyCode: { errorMap: { onSubmit: 'API error' } },
    } as unknown as Record<string, AnyFieldLikeMetaBase>
    const form = {
      state: { fieldMeta },
      setFieldMeta: (name: string, updater: Updater<AnyFieldLikeMetaBase>) => {
        if (typeof updater !== 'function') {
          throw new Error('Expected an updater function')
        }
        fieldMeta[name] = updater(fieldMeta[name])
        calls.push('clear')
      },
      validate: vi.fn(async () => {
        calls.push('validate')
      }),
      handleSubmit: vi.fn(async () => {
        calls.push('submit')
      }),
    }

    await submitForm(form as unknown as AnyFormApi, { returnToTaskList: true })

    expect(calls).toEqual(['clear', 'validate', 'submit'])
    expect(form.validate).toHaveBeenCalledWith('submit')
    expect(form.handleSubmit).toHaveBeenCalledWith({ returnToTaskList: true })
  })
})
