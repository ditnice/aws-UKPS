import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { resetPasswordAction } from './resetPasswordAction'
import { passwordRestFormOptions, resetEmailCookieName } from './resetPasswordShared'

type FormData = typeof passwordRestFormOptions.defaultValues
type ServerValidator = (props: { value: FormData }) => Promise<string | undefined>

const mocks = vi.hoisted(() => ({
  initiateForgottenPasswordProcedure: vi.fn(),
  createServerApiClient: vi.fn(),
  serverValidate: vi.fn(),
  redirect: vi.fn(),
  createResetPasswordCodeHref: vi.fn(),
  cookieSet: vi.fn(),
  isValidationProblemDetails: vi.fn(),
  getFormStateServerErrors: vi.fn(),
}))

vi.mock('@/client/generated', () => ({
  initiateForgottenPasswordProcedure: mocks.initiateForgottenPasswordProcedure,
}))
vi.mock('@/client/server-api', () => ({
  createServerApiClient: mocks.createServerApiClient,
}))
vi.mock('./resetPasswordShared', async (importOriginal) => ({
  ...(await importOriginal<typeof import('./resetPasswordShared')>()),
  serverValidate: mocks.serverValidate,
}))
vi.mock('next/navigation', () => ({ redirect: mocks.redirect }))
vi.mock('./confirm/navigation', () => ({
  createResetPasswordCodeHref: mocks.createResetPasswordCodeHref,
}))
vi.mock('next/headers', () => ({
  cookies: async () => ({ set: mocks.cookieSet }),
}))
vi.mock('@/lib/responses/typeGuards', () => ({
  isValidationProblemDetails: mocks.isValidationProblemDetails,
}))
vi.mock('@/lib/form/formErrorHandling', () => ({
  getFormStateServerErrors: mocks.getFormStateServerErrors,
}))

const GENERIC_ERROR =
  'An unexpected error occurred while attempting to reset your password. Please try again.'
const RATE_LIMIT_ERROR = 'Number of requests exceeded limit. Try again later.'

const values = { email: 'user@example.com' }
const apiClient = { name: 'api-client' }
const confirmHref = '/reset-password/confirm'

const makeFormData = () => {
  const formData = new FormData()
  formData.set('email', values.email)
  return formData
}

const run = () => resetPasswordAction(undefined, makeFormData())

/** Configure what the API call resolves with. Defaults to a successful response. */
const givenApiResult = (
  result: { error?: unknown; response?: { ok: boolean; status: number } | undefined } = {},
) => {
  const hasResponse = 'response' in result
  mocks.initiateForgottenPasswordProcedure.mockResolvedValue({
    error: undefined,
    ...result,
    response: hasResponse ? result.response : { ok: true, status: 200 },
  })
}

const expectNoCookieOrRedirect = () => {
  expect(mocks.cookieSet).not.toHaveBeenCalled()
  expect(mocks.redirect).not.toHaveBeenCalled()
}

describe('resetPasswordAction', () => {
  beforeEach(() => {
    vi.resetAllMocks()
    mocks.serverValidate.mockResolvedValue(values)
    mocks.createServerApiClient.mockResolvedValue(apiClient)
    mocks.createResetPasswordCodeHref.mockReturnValue(confirmHref)
    mocks.isValidationProblemDetails.mockReturnValue(false)
    givenApiResult()
  })

  describe('on success', () => {
    it('calls the API with the validated values and a server client', async () => {
      await run()

      expect(mocks.serverValidate).toHaveBeenCalledWith(expect.any(FormData))
      expect(mocks.initiateForgottenPasswordProcedure).toHaveBeenCalledWith({
        client: apiClient,
        body: values,
      })
    })

    it('stores the email in a secure, httpOnly cookie for 30 minutes', async () => {
      await run()

      expect(mocks.cookieSet).toHaveBeenCalledWith(resetEmailCookieName, values.email, {
        httpOnly: true,
        secure: true,
        sameSite: 'lax',
        path: '/',
        maxAge: 60 * 30,
      })
    })

    it('redirects to the confirm page after setting the cookie', async () => {
      await run()

      expect(mocks.redirect).toHaveBeenCalledExactlyOnceWith(confirmHref)
      expect(mocks.cookieSet.mock.invocationCallOrder[0]).toBeLessThan(
        mocks.redirect.mock.invocationCallOrder[0],
      )
    })
  })

  describe('when the API returns validation problem details', () => {
    const problem = { errors: { email: ['Email is invalid'] } }
    const serverErrors = { errorMap: { onServer: { fields: { email: 'Email is invalid' } } } }

    beforeEach(() => {
      givenApiResult({ error: problem, response: { ok: false, status: 400 } })
      mocks.isValidationProblemDetails.mockReturnValue(true)
      mocks.getFormStateServerErrors.mockReturnValue(serverErrors)
    })

    it('returns the submitted values merged with the server errors', async () => {
      const result = await run()

      expect(mocks.getFormStateServerErrors).toHaveBeenCalledWith(problem.errors)
      expect(result).toEqual({ ...initialFormState, values, ...serverErrors })
    })

    it('does not set the cookie or redirect', async () => {
      await run()
      expectNoCookieOrRedirect()
    })
  })

  describe.each([
    {
      name: 'rate limited (429)',
      result: { error: { title: 'Too many' }, response: { ok: false, status: 429 } },
      message: RATE_LIMIT_ERROR,
    },
    {
      name: 'server error (500)',
      result: { error: { title: 'Boom' }, response: { ok: false, status: 500 } },
      message: GENERIC_ERROR,
    },
    {
      name: 'no response (network failure)',
      result: { error: new Error('fetch failed'), response: undefined },
      message: GENERIC_ERROR,
    },
    {
      name: 'non-ok response without an error body',
      result: { error: undefined, response: { ok: false, status: 502 } },
      message: GENERIC_ERROR,
    },
    {
      name: 'error body alongside an ok response',
      result: { error: { title: 'Odd' }, response: { ok: true, status: 200 } },
      message: GENERIC_ERROR,
    },
    {
      name: 'missing response without an error',
      result: { error: undefined, response: undefined },
      message: GENERIC_ERROR,
    },
  ])('when the API call fails: $name', ({ result, message }) => {
    beforeEach(() => givenApiResult(result))

    it('returns the expected form-level error', async () => {
      expect(await run()).toEqual({ ...initialFormState, errors: [message] })
    })

    it('does not set the cookie or redirect', async () => {
      await run()
      expectNoCookieOrRedirect()
    })
  })

  describe('when serverValidate throws a ServerValidateError', () => {
    const formState = { ...initialFormState, errors: ['Invalid email'] }

    beforeEach(() => {
      mocks.serverValidate.mockRejectedValue(
        new ServerValidateError<FormData, ServerValidator>({
          formState,
        }),
      )
    })

    it('returns its form state without calling the API', async () => {
      expect(await run()).toBe(formState)
      expect(mocks.initiateForgottenPasswordProcedure).not.toHaveBeenCalled()
      expectNoCookieOrRedirect()
    })
  })

  describe.each([
    {
      name: 'serverValidate throws an unexpected error',
      arrange: (error: Error) => mocks.serverValidate.mockRejectedValue(error),
    },
    {
      name: 'creating the API client throws',
      arrange: (error: Error) => mocks.createServerApiClient.mockRejectedValue(error),
    },
    {
      name: 'the API call rejects',
      arrange: (error: Error) => mocks.initiateForgottenPasswordProcedure.mockRejectedValue(error),
    },
  ])('unexpected errors: $name', ({ arrange }) => {
    it('rethrows the error without setting the cookie or redirecting', async () => {
      const error = new Error('kaboom')
      arrange(error)

      await expect(run()).rejects.toBe(error)
      expectNoCookieOrRedirect()
    })
  })
})
