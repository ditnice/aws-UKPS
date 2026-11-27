import { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'

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

/**
 * Puts a validation problem's errors on the fields they are keyed to. Those keyed to one of
 * `fieldNames`, the fields that show their own errors, are left out of the form-level errors so
 * they are not shown twice.
 */
export const getFormStateServerErrors = (errors: Errors, fieldNames: string[] = []) => {
  const fields = lowerCamelCaseKeys(errors)
  return {
    errorMap: { onServer: { fields } },
    errors: Object.entries(fields)
      .filter(([key]) => !fieldNames.includes(key))
      .flatMap(([, messages]) => messages),
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
