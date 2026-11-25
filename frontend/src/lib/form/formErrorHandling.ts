import { AnyFieldLikeMetaBase, AnyFormApi, Updater } from '@tanstack/react-form'

import { ValidationProblemDetails } from '@/client/generated'

export type Errors = { [key: string]: string[] }
export const lowerCamelCaseKeys = (errors: Errors): Errors => {
  return Object.fromEntries(
    Object.entries(errors).map(([key, value]) => [
      key.charAt(0).toLowerCase() + key.slice(1),
      value,
    ]),
  )
}

export const getFormStateServerErrors = (errors: Errors) => {
  return {
    errorMap: { onServer: { fields: lowerCamelCaseKeys(errors) } },
    errors: Object.values(errors).flat(),
  }
}

export const updateFormApiErrors = (
  validationProblemDetails: ValidationProblemDetails,
  key: string,
): Updater<AnyFieldLikeMetaBase> => {
  return (meta) => ({
    ...meta,
    errorMap: {
      ...meta.errorMap,
      onSubmit: validationProblemDetails.errors[key],
    },
  })
}

export const clearFormApiErrors = (): Updater<AnyFieldLikeMetaBase> => {
  return (meta) => ({
    ...meta,
    errorMap: {
      ...meta.errorMap,
      onSubmit: undefined,
    },
  })
}

/**
 * Clears the errors the server set on any field, before the form is submitted again. Only a change
 * to the field removes them otherwise, so resubmitting an unchanged field would be blocked.
 */
export const clearAllFormApiErrors = (form: AnyFormApi) => {
  for (const name of Object.keys(form.state.fieldMeta)) {
    form.setFieldMeta(name, clearFormApiErrors())
  }
}

/**
 * Submits a form that validates on submit only. Before submitting, the form checks each field for
 * errors, but only re-runs its own validation afterwards, so errors left by the previous attempt
 * would stop a corrected form from submitting. This clears the server's errors and re-runs the
 * form's validation first.
 */
export const submitForm = async (form: AnyFormApi, meta?: unknown) => {
  clearAllFormApiErrors(form)
  await form.validate('submit')
  await form.handleSubmit(meta)
}
