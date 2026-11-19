import { beforeEach, describe, expect, it, vi } from 'vitest'

import { resendVerificationEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

// Adjust this import to wherever the server action lives
import { resendWorkEmailVerification } from './actions'

vi.mock('@/client/generated', () => ({
  resendVerificationEmail: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

const mockResend = vi.mocked(resendVerificationEmail)
const mockCreateClient = vi.mocked(createServerApiClient)

const FALLBACK_MESSAGE = 'An unexpected error occurred.'

// The generated client's return type is complex; tests only care about
// `response` and `error`, so we cast the partial results.
const apiResult = (result: { response?: Partial<Response>; error?: unknown }) =>
  result as unknown as Awaited<ReturnType<typeof resendVerificationEmail>>

describe('resendWorkEmailVerification', () => {
  const fakeClient = { fake: 'client' } as unknown as Awaited<
    ReturnType<typeof createServerApiClient>
  >
  const userId = 42
  const formData = new FormData()

  beforeEach(() => {
    vi.clearAllMocks()
    mockCreateClient.mockResolvedValue(fakeClient)
  })

  it('returns success when the response is ok', async () => {
    mockResend.mockResolvedValue(apiResult({ response: { ok: true } }))

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({ status: 'success' })
  })

  it('calls the API with the userId path param and the server client', async () => {
    mockResend.mockResolvedValue(apiResult({ response: { ok: true } }))

    await resendWorkEmailVerification(userId, undefined, formData)

    expect(mockCreateClient).toHaveBeenCalledTimes(1)
    expect(mockResend).toHaveBeenCalledTimes(1)
    expect(mockResend).toHaveBeenCalledWith({
      path: { userId },
      client: fakeClient,
    })
  })

  it('returns the error title when the response is not ok', async () => {
    mockResend.mockResolvedValue(
      apiResult({
        response: { ok: false },
        error: { title: 'Email already verified' },
      }),
    )

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({
      status: 'error',
      message: 'Email already verified',
    })
  })

  it('falls back to a generic message when the error has no title', async () => {
    mockResend.mockResolvedValue(apiResult({ response: { ok: false }, error: {} }))

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({ status: 'error', message: FALLBACK_MESSAGE })
  })

  it('falls back to a generic message when the response is not ok and there is no error', async () => {
    mockResend.mockResolvedValue(apiResult({ response: { ok: false } }))

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({ status: 'error', message: FALLBACK_MESSAGE })
  })

  it('falls back to a generic message when there is no response at all', async () => {
    mockResend.mockResolvedValue(apiResult({}))

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({ status: 'error', message: FALLBACK_MESSAGE })
  })

  it('prefers the error title over the fallback even when the response is missing', async () => {
    mockResend.mockResolvedValue(apiResult({ error: { title: 'Service unavailable' } }))

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({
      status: 'error',
      message: 'Service unavailable',
    })
  })

  it('returns a generic error when the API call throws (e.g. network failure)', async () => {
    mockResend.mockRejectedValue(new Error('Network down'))

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({ status: 'error', message: FALLBACK_MESSAGE })
  })

  it('returns a generic error when creating the server API client throws', async () => {
    mockCreateClient.mockRejectedValue(new Error('No session'))

    const result = await resendWorkEmailVerification(userId, undefined, formData)

    expect(result).toEqual({ status: 'error', message: FALLBACK_MESSAGE })
    expect(mockResend).not.toHaveBeenCalled()
  })

  it('ignores prevState and formData', async () => {
    mockResend.mockResolvedValue(apiResult({ response: { ok: true } }))

    const result = await resendWorkEmailVerification(
      userId,
      { status: 'error', message: 'previous failure' },
      new FormData(),
    )

    expect(result).toEqual({ status: 'success' })
  })
})
