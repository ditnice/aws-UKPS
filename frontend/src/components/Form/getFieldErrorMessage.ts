export function getFieldErrorMessage(errors: unknown[]): string | undefined {
  const firstError = errors[0]

  if (!firstError) {
    return undefined
  }

  if (typeof firstError === 'string') {
    return firstError
  }

  if (typeof firstError === 'object' && 'message' in firstError) {
    return String(firstError.message)
  }

  return undefined
}
