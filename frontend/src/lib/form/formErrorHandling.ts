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

export const getFormStateServerErrors = (errors: Errors) => {
  return {
    errorMap: { onServer: { fields: lowerCamelCaseKeys(errors) } },
    errors: Object.values(errors),
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
