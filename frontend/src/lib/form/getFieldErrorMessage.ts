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

export function getFieldErrorMessage(errors: unknown[]): string | undefined {
  const firstError = errors[0]

  return extractErrorMessage(firstError)
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
