import type { ValidationProblemDetails } from '@/client/generated'
import { updateFormApiErrors } from '@/lib/form/formErrorHandling'

import type { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'

type FormApiLike = {
  setFieldMeta: (field: never, updater: Updater<AnyFieldLikeMetaBase>) => void
}

/**
 * Attaches server validation errors to their fields. Error keys are question IDs, which are
 * also the field names, so no mapping is needed. Returns messages for keys that are not
 * questions on this page, to show in the error summary instead.
 */
export const applyServerErrors = (
  formApi: FormApiLike,
  error: ValidationProblemDetails,
  questionIds: string[],
): string[] => {
  const unmatched: string[] = []
  for (const [key, messages] of Object.entries(error.errors)) {
    if (questionIds.includes(key)) {
      formApi.setFieldMeta(key as never, updateFormApiErrors(error, key))
    } else {
      unmatched.push(...messages)
    }
  }
  return unmatched
}
