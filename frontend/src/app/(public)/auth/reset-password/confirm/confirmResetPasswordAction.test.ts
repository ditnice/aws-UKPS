import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { redirect } from 'next/navigation'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { confirmNewPassword } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { getFormStateServerErrors } from '@/lib/form/formErrorHandling'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { createPasswordUpdateSuccessHref } from '../success/navigation'

import { confirmResetPasswordAction } from './confirmResetPasswordAction'
import { confirmPasswordResetFormOptions, serverValidate } from './confirmResetPasswordShared'

vi.mock('@/client/generated', () => ({ confirmNewPassword: vi.fn() }))
vi.mock('@/client/server-api', () => ({ createServerApiClient: vi.fn() }))
vi.mock('@/lib/responses/typeGuards', () => ({ isValidationProblemDetails: vi.fn() }))
vi.mock('@/lib/form/formErrorHandling', () => ({ getFormStateServerErrors: vi.fn() }))
vi.mock('../success/navigation', () => ({ createPasswordUpdateSuccessHref: vi.fn() }))
vi.mock('next/navigation', () => ({ redirect: vi.fn() }))
vi.mock('./confirmResetPasswordShared', async (importOriginal) => ({
  ...(await importOriginal<typeof import('./confirmResetPasswordShared')>()),
  serverValidate: vi.fn(),
}))

const STATUS_OK = 200
const STATUS_TOO_MANY_REQUESTS = 429
const STATUS_SERVER_ERROR = 500

// Next's redirect() throws; mirror that so we can prove the action reaches it
// and doesn't swallow it.
class RedirectSignal extends Error {}

const mocked = {
  confirmNewPassword: vi.mocked(confirmNewPassword),
  createServerApiClient: vi.mocked(createServerApiClient),
  isValidationProblemDetails: vi.mocked(isValidationProblemDetails),
  getFormStateServerErrors: vi.mocked(getFormStateServerErrors),
  createPasswordUpdateSuccessHref: vi.mocked(createPasswordUpdateSuccessHref),
  redirect: vi.mocked(redirect),
  serverValidate: vi.mocked(serverValidate),
}

const values = { ...confirmPasswordResetFormOptions.defaultValues }
const client = { name: 'api-client' } as unknown as Awaited<
  ReturnType<typeof createServerApiClient>
>
const successHref = createSuccessHrefPlaceholder()
const formData = new FormData()

function createSuccessHrefPlaceholder() {
  return `${Math.random()}-success-href`
}

// Cast helper: the generated client's return type is irrelevant to this unit.
const apiResult = (result: { error?: unknown; status?: number | null }) =>
  ({
    error: result.error,
    response: result.status == null ? undefined : new Response(null, { status: result.status }),
  }) as unknown as Awaited<ReturnType<typeof confirmNewPassword>>

const run = () => confirmResetPasswordAction(undefined, formData)

/** Runs the action for an API failure and returns the single error message produced. */
const getErrorMessageFor = async (status: number | null) => {
  mocked.confirmNewPassword.mockResolvedValue(apiResult({ error: { detail: 'x' }, status }))
  const state = await run()
  expect(state.errors).toHaveLength(1)
  return state.errors![0] as string
}

describe('confirmResetPasswordAction', () => {
  beforeEach(() => {
    vi.resetAllMocks()
    mocked.serverValidate.mockResolvedValue(values)
    mocked.createServerApiClient.mockResolvedValue(client)
    mocked.isValidationProblemDetails.mockReturnValue(false)
    mocked.createPasswordUpdateSuccessHref.mockReturnValue(successHref)
    mocked.redirect.mockImplementation(() => {
      throw new RedirectSignal()
    })
  })

  describe('on success', () => {
    beforeEach(() => {
      mocked.confirmNewPassword.mockResolvedValue(apiResult({ status: STATUS_OK }))
    })

    it('validates the submitted form data', async () => {
      await expect(run()).rejects.toThrow(RedirectSignal)

      expect(mocked.serverValidate).toHaveBeenCalledExactlyOnceWith(formData)
    })

    it('sends the validated values using a server API client', async () => {
      await expect(run()).rejects.toThrow(RedirectSignal)

      expect(mocked.confirmNewPassword).toHaveBeenCalledExactlyOnceWith({
        client,
        body: values,
      })
    })

    it('redirects to the password update success page', async () => {
      await expect(run()).rejects.toThrow(RedirectSignal)

      expect(mocked.redirect).toHaveBeenCalledExactlyOnceWith(successHref)
    })
  })

  describe('when the API returns validation problem details', () => {
    const problem = { errors: { password: ['too weak'] } }
    const serverErrors = { errorMap: { onServer: { fields: {} } } }

    beforeEach(() => {
      mocked.confirmNewPassword.mockResolvedValue(
        apiResult({ error: problem, status: STATUS_SERVER_ERROR }),
      )
      mocked.isValidationProblemDetails.mockReturnValue(true)
      mocked.getFormStateServerErrors.mockReturnValue(serverErrors as never)
    })

    it('returns the submitted values merged with the server field errors', async () => {
      const state = await run()

      expect(mocked.isValidationProblemDetails).toHaveBeenCalledWith(problem)
      expect(mocked.getFormStateServerErrors).toHaveBeenCalledWith(problem.errors)
      expect(state).toEqual({ ...initialFormState, values, ...serverErrors })
    })

    it('does not redirect', async () => {
      await run()

      expect(mocked.redirect).not.toHaveBeenCalled()
    })
  })

  describe('when the API call fails', () => {
    it.each([
      ['an error with a response', { error: {}, status: STATUS_SERVER_ERROR }],
      ['an error with no response', { error: {}, status: null }],
      ['a non-ok response with no error', { error: undefined, status: STATUS_SERVER_ERROR }],
      ['no error and no response', { error: undefined, status: null }],
    ])('returns a form-level error and does not redirect for %s', async (_name, result) => {
      mocked.confirmNewPassword.mockResolvedValue(apiResult(result))

      const state = await run()

      expect(state).toEqual({ ...initialFormState, errors: [expect.any(String)] })
      expect(mocked.redirect).not.toHaveBeenCalled()
    })

    it('uses a rate-limit specific message for a 429, distinct from the generic one', async () => {
      const rateLimited = await getErrorMessageFor(STATUS_TOO_MANY_REQUESTS)
      const generic = await getErrorMessageFor(STATUS_SERVER_ERROR)

      expect(rateLimited).toBeTruthy()
      expect(generic).toBeTruthy()
      expect(rateLimited).not.toBe(generic)
    })

    it('uses the same generic message regardless of status or missing response', async () => {
      const generic = await getErrorMessageFor(STATUS_SERVER_ERROR)

      expect(await getErrorMessageFor(null)).toBe(generic)
    })
  })

  describe('error handling', () => {
    it('returns the form state from a ServerValidateError', async () => {
      const formState = { ...initialFormState, values }
      mocked.serverValidate.mockRejectedValue(
        new ServerValidateError({ formState } as ConstructorParameters<
          typeof ServerValidateError
        >[0]),
      )

      await expect(run()).resolves.toBe(formState)
      expect(mocked.confirmNewPassword).not.toHaveBeenCalled()
      expect(mocked.redirect).not.toHaveBeenCalled()
    })

    it.each([
      ['serverValidate', () => mocked.serverValidate],
      ['confirmNewPassword', () => mocked.confirmNewPassword],
      ['createServerApiClient', () => mocked.createServerApiClient],
    ])('rethrows unexpected errors from %s', async (_name, getMock) => {
      const failure = new Error('boom')
      getMock().mockRejectedValue(failure)

      await expect(run()).rejects.toBe(failure)
      expect(mocked.redirect).not.toHaveBeenCalled()
    })
  })
})
