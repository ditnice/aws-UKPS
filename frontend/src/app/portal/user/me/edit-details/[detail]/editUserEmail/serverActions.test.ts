import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { errorMessages } from '@/lib/form/errorMessages'

import { updateUserEmailAction } from './serverActions'

// ---- Mocks -----------------------------------------------------------------

const {
  redirectMock,
  updateUserEmailMock,
  createServerApiClientMock,
  serverValidateMock,
  getFormStateServerErrorsMock,
  isValidationProblemDetailsMock,
} = vi.hoisted(() => ({
  redirectMock: vi.fn(),
  updateUserEmailMock: vi.fn(),
  createServerApiClientMock: vi.fn(),
  serverValidateMock: vi.fn(),
  getFormStateServerErrorsMock: vi.fn(),
  isValidationProblemDetailsMock: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  redirect: redirectMock,
}))

vi.mock('@/client/generated', () => ({
  updateUserEmail: updateUserEmailMock,
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: createServerApiClientMock,
}))

vi.mock('@/lib/form/formErrorHandling', () => ({
  getFormStateServerErrors: getFormStateServerErrorsMock,
}))

vi.mock('@/lib/responses/typeGuards', () => ({
  isValidationProblemDetails: isValidationProblemDetailsMock,
}))

vi.mock('./shared', () => ({
  serverValidate: serverValidateMock,
}))

// ---- Tests -----------------------------------------------------------------

describe('updateUserEmailAction', () => {
  const userId = 123

  const values = {
    email: 'new.email@example.com',
  }

  const formData = new FormData()

  const apiClient = {}

  beforeEach(() => {
    vi.clearAllMocks()

    createServerApiClientMock.mockResolvedValue(apiClient)
    serverValidateMock.mockResolvedValue(values)
    updateUserEmailMock.mockResolvedValue({
      response: { ok: true },
    })
    isValidationProblemDetailsMock.mockReturnValue(false)
  })

  it('updates the email and redirects on success', async () => {
    await updateUserEmailAction(userId, undefined, formData)

    expect(serverValidateMock).toHaveBeenCalledWith(formData)

    expect(createServerApiClientMock).toHaveBeenCalledOnce()

    expect(updateUserEmailMock).toHaveBeenCalledWith({
      client: apiClient,
      path: { userId },
      body: values,
    })

    expect(redirectMock).toHaveBeenCalledWith('/portal/user/me?updated=true')
  })

  it('returns an email conflict error when the API responds with 409', async () => {
    updateUserEmailMock.mockResolvedValue({
      response: {
        ok: false,
        status: 409,
      },
    })

    const result = await updateUserEmailAction(userId, undefined, formData)

    expect(result).toEqual({
      ...initialFormState,
      values,
      errorMap: {
        onServer: {
          fields: {
            value: 'Provided email address conflicts with an existing email.',
          },
        },
      },
    })

    expect(redirectMock).not.toHaveBeenCalled()
  })

  it('returns mapped form errors when the API responds with validation errors', async () => {
    const validationErrors = {
      Email: ['The email address is invalid.'],
    }

    const formErrors = {
      errorMap: {
        onServer: {
          fields: {
            email: 'The email address is invalid.',
          },
        },
      },
    }

    const error = { errors: validationErrors }

    updateUserEmailMock.mockResolvedValue({
      response: {
        ok: false,
        status: 400,
      },
      error,
    })

    isValidationProblemDetailsMock.mockReturnValue(true)
    getFormStateServerErrorsMock.mockReturnValue(formErrors)

    const result = await updateUserEmailAction(userId, undefined, formData)

    expect(isValidationProblemDetailsMock).toHaveBeenCalledWith(error)

    expect(getFormStateServerErrorsMock).toHaveBeenCalledWith(validationErrors)

    expect(result).toEqual({
      ...initialFormState,
      values,
      ...formErrors,
    })

    expect(redirectMock).not.toHaveBeenCalled()
  })

  it('returns a generic error when the API responds with an unexpected error', async () => {
    updateUserEmailMock.mockResolvedValue({
      response: {
        ok: false,
        status: 500,
      },
      error: {
        message: 'Internal server error',
      },
    })

    const result = await updateUserEmailAction(userId, undefined, formData)

    expect(result).toEqual({
      ...initialFormState,
      errors: [errorMessages.genericWithRetry],
    })

    expect(redirectMock).not.toHaveBeenCalled()
  })

  it('returns a generic error when the API response is unsuccessful without an error', async () => {
    updateUserEmailMock.mockResolvedValue({
      response: {
        ok: false,
        status: 500,
      },
    })

    const result = await updateUserEmailAction(userId, undefined, formData)

    expect(result).toEqual({
      ...initialFormState,
      errors: [errorMessages.genericWithRetry],
    })
  })

  it('returns the form state when server validation fails', async () => {
    const formState = {
      values,
      errors: ['Enter a valid email address.'],
    }

    serverValidateMock.mockRejectedValue(new ServerValidateError({ formState } as never))

    const result = await updateUserEmailAction(userId, undefined, formData)

    expect(result).toEqual(formState)

    expect(createServerApiClientMock).not.toHaveBeenCalled()
    expect(updateUserEmailMock).not.toHaveBeenCalled()
  })

  it('rethrows unexpected exceptions', async () => {
    const error = new Error('Something went wrong')

    serverValidateMock.mockRejectedValue(error)

    await expect(updateUserEmailAction(userId, undefined, formData)).rejects.toThrow(error)

    expect(createServerApiClientMock).not.toHaveBeenCalled()
    expect(updateUserEmailMock).not.toHaveBeenCalled()
  })
})
