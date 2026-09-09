import { ValidationProblemDetails } from '@/client/generated'

export const isValidationProblemDetails = (value: unknown): value is ValidationProblemDetails => {
  if (!value || typeof value !== 'object') {
    return false
  }

  const candidate = value as Record<string, unknown>

  if (
    !candidate.errors ||
    typeof candidate.errors !== 'object' ||
    Array.isArray(candidate.errors)
  ) {
    return false
  }

  return Object.values(candidate.errors).every(
    (error) => Array.isArray(error) && error.every((message) => typeof message === 'string'),
  )
}
