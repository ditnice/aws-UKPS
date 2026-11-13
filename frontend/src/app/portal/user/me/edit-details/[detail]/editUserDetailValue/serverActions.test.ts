import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { redirect } from 'next/navigation'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { updateUserDetails } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFormStateServerErrors } from '@/lib/form/formErrorHandling'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { updateUserDetailsAction } from './serverActions'
import { serverValidate } from './shared'

vi.mock('next/navigation', () => ({
  redirect: vi.fn(),
}))

vi.mock('@/client/generated', () => ({
  updateUserDetails: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('@/lib/form/formErrorHandling', () => ({
  getFormStateServerErrors: vi.fn(),
}))

vi.mock('@/lib/responses/typeGuards', () => ({
  isValidationProblemDetails: vi.fn(),
}))

vi.mock('./shared', () => ({
  serverValidate: vi.fn(),
}))

describe('updateUserDetailsAction', () => {
  const userId = 42
  const formData = new FormData()
  const values = { firstName: 'Jane', lastName: 'Doe' }
  const fakeClient = { id: 'fake-client' }

  beforeEach(() => {
    vi.resetAllMocks()

    vi.mocked(serverValidate).mockResolvedValue(values as never)
    vi.mocked(createServerApiClient).mockResolvedValue(fakeClient as never)
    vi.mocked(isValidationProblemDetails).mockReturnValue(false)
    // Real Next.js redirect() throws; mimic that so code after it doesn't run
    vi.mocked(redirect).mockImplementation(() => {
      throw new Error('NEXT_REDIRECT')
    })
  })

  describe('validation', () => {
    it('validates the submitted form data', async () => {
      vi.mocked(updateUserDetails).mockResolvedValue({
        error: undefined,
        response: { ok: true },
      } as never)

      await expect(updateUserDetailsAction(userId, undefined, formData)).rejects.toThrow(
        'NEXT_REDIRECT',
      )

      expect(serverValidate).toHaveBeenCalledWith(formData)
    })

    it('returns the form state and skips the API call when server validation fails', async () => {
      const formState = { ...initialFormState, errors: ['Invalid'] }
      vi.mocked(serverValidate).mockRejectedValue(new ServerValidateError({ formState } as never))

      const result = await updateUserDetailsAction(userId, undefined, formData)

      expect(result).toBe(formState)
      expect(updateUserDetails).not.toHaveBeenCalled()
      expect(redirect).not.toHaveBeenCalled()
    })

    it('rethrows errors that are not ServerValidateError', async () => {
      const error = new Error('boom')
      vi.mocked(serverValidate).mockRejectedValue(error)

      await expect(updateUserDetailsAction(userId, undefined, formData)).rejects.toBe(error)

      expect(updateUserDetails).not.toHaveBeenCalled()
    })
  })

  describe('API call', () => {
    it('calls updateUserDetails with the server client, user id and validated values', async () => {
      vi.mocked(updateUserDetails).mockResolvedValue({
        error: undefined,
        response: { ok: true },
      } as never)

      await expect(updateUserDetailsAction(userId, undefined, formData)).rejects.toThrow(
        'NEXT_REDIRECT',
      )

      expect(createServerApiClient).toHaveBeenCalledTimes(1)
      expect(updateUserDetails).toHaveBeenCalledWith({
        client: fakeClient,
        path: { userId },
        body: values,
      })
    })
  })

  describe('success', () => {
    it('redirects to the user profile page with updated=true', async () => {
      vi.mocked(updateUserDetails).mockResolvedValue({
        error: undefined,
        response: { ok: true },
      } as never)

      await expect(updateUserDetailsAction(userId, undefined, formData)).rejects.toThrow(
        'NEXT_REDIRECT',
      )

      expect(redirect).toHaveBeenCalledWith('/portal/user/me?updated=true')
    })

    it('does not treat a redirect as an error response', async () => {
      vi.mocked(updateUserDetails).mockResolvedValue({
        error: undefined,
        response: { ok: true },
      } as never)

      await expect(updateUserDetailsAction(userId, undefined, formData)).rejects.toThrow(
        'NEXT_REDIRECT',
      )

      expect(isValidationProblemDetails).not.toHaveBeenCalled()
    })
  })

  describe('API validation errors', () => {
    it('returns the submitted values merged with mapped server errors', async () => {
      const apiErrors = { firstName: ['First name is required'] }
      const mappedErrors = {
        errors: [],
        errorMap: { onServer: { fields: apiErrors } },
      }

      vi.mocked(updateUserDetails).mockResolvedValue({
        error: { errors: apiErrors },
        response: { ok: false },
      } as never)
      vi.mocked(isValidationProblemDetails).mockReturnValue(true)
      vi.mocked(getFormStateServerErrors).mockReturnValue(mappedErrors as never)

      const result = await updateUserDetailsAction(userId, undefined, formData)

      expect(getFormStateServerErrors).toHaveBeenCalledWith(apiErrors)
      expect(result).toEqual({ ...initialFormState, values, ...mappedErrors })
      expect(redirect).not.toHaveBeenCalled()
    })
  })

  describe('unexpected errors', () => {
    it('returns a generic error when the API returns a non-validation error', async () => {
      vi.mocked(updateUserDetails).mockResolvedValue({
        error: { title: 'Internal Server Error' },
        response: { ok: false },
      } as never)

      const result = await updateUserDetailsAction(userId, undefined, formData)

      expect(result).toEqual({ ...initialFormState, errors: [errorMessages.genericWithRetry] })
      expect(getFormStateServerErrors).not.toHaveBeenCalled()
      expect(redirect).not.toHaveBeenCalled()
    })

    it('returns a generic error when the response is not ok and there is no error body', async () => {
      vi.mocked(updateUserDetails).mockResolvedValue({
        error: undefined,
        response: { ok: false },
      } as never)

      const result = await updateUserDetailsAction(userId, undefined, formData)

      expect(result).toEqual({ ...initialFormState, errors: [errorMessages.genericWithRetry] })
    })

    it('returns a generic error when there is no response at all', async () => {
      vi.mocked(updateUserDetails).mockResolvedValue({
        error: undefined,
        response: undefined,
      } as never)

      const result = await updateUserDetailsAction(userId, undefined, formData)

      expect(result).toEqual({ ...initialFormState, errors: [errorMessages.genericWithRetry] })
    })

    it('rethrows when the API call itself throws', async () => {
      const error = new Error('network down')
      vi.mocked(updateUserDetails).mockRejectedValue(error)

      await expect(updateUserDetailsAction(userId, undefined, formData)).rejects.toBe(error)
    })

    it('rethrows when creating the API client fails', async () => {
      const error = new Error('no session')
      vi.mocked(createServerApiClient).mockRejectedValue(error)

      await expect(updateUserDetailsAction(userId, undefined, formData)).rejects.toBe(error)
      expect(updateUserDetails).not.toHaveBeenCalled()
    })
  })
})
