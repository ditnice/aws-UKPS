import { ValidationProblemDetails } from '@/client/generated'

export const updateFormApiErrors = (
  validationProblemDetails: ValidationProblemDetails,
  key: string,
): import('@tanstack/react-form').Updater<import('@tanstack/react-form').AnyFieldLikeMetaBase> => {
  return (meta) => ({
    ...meta,
    errorMap: {
      ...meta.errorMap,
      onSubmit: validationProblemDetails.errors[key],
    },
  })
}
