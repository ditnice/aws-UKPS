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
