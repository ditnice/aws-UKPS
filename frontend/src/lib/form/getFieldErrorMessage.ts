export const extractErrorMessage = (error: unknown): string | undefined => {
  if (!error) {
    return undefined
  }

  if (typeof error === 'string') {
    return error
  }

  if (typeof error === 'object' && 'message' in error) {
    return String(error.message)
  }

  return undefined
}

export function getFormErrorMessage(submitError: unknown): string | undefined {
  if (typeof submitError === 'string') return submitError

  if (
    typeof submitError === 'object' &&
    submitError !== null &&
    'form' in submitError &&
    typeof submitError.form === 'string'
  ) {
    return submitError.form
  }

  return undefined
}

export function getFieldErrorMessage(errors: unknown[]): string | undefined {
  const firstError = errors[0]
  const itemsOnly =
    firstError && typeof firstError === 'object' && 'params' in firstError
      ? (firstError.params as { itemsOnly?: unknown } | undefined)?.itemsOnly
      : undefined

  return itemsOnly ? undefined : extractErrorMessage(firstError)
}

/** The parts of an answer the first error is about, when it names them in its `params`. */
export function getFieldErrorParts(errors: unknown[]): string[] | undefined {
  const firstError = errors[0]
  const parts =
    firstError && typeof firstError === 'object' && 'params' in firstError
      ? (firstError.params as { parts?: unknown } | undefined)?.parts
      : undefined

  return Array.isArray(parts) ? parts.filter((part) => typeof part === 'string') : undefined
}

/**
 * The errors of a text list's entries or a group's items, when the first error names them in its
 * `params`, keyed as in `QuestionError.items` (lib/dynamic-form/answers.ts).
 */
export function getFieldErrorItems(errors: unknown[]): Record<string, string> | undefined {
  const firstError = errors[0]
  const items =
    firstError && typeof firstError === 'object' && 'params' in firstError
      ? (firstError.params as { items?: unknown } | undefined)?.items
      : undefined

  if (!items || typeof items !== 'object') {
    return undefined
  }
  return Object.fromEntries(
    Object.entries(items).filter(
      (entry): entry is [string, string] => typeof entry[1] === 'string',
    ),
  )
}
