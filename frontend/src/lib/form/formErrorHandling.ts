import { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'

import { ValidationProblemDetails } from '@/client/generated'

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
