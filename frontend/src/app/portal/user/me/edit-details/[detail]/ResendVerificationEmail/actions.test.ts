import { beforeEach, describe, expect, it, vi } from 'vitest'

import { resendVerificationEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import { buildUpdateUserDetailsHref } from '../navigation'

import { resendWorkEmailVerification } from './actions' // <- adjust to your file name

// Mimic Next's control-flow errors: redirect() throws, unstable_rethrow() re-throws them.
class NextControlFlowError extends Error {
  digest = 'NEXT_REDIRECT'
}

vi.mock('next/navigation', () => ({
  redirect: vi.fn((url: string) => {
    throw Object.assign(new NextControlFlowError(url), { url })
  }),
  unstable_rethrow: vi.fn((error: unknown) => {
    if (error instanceof NextControlFlowError) throw error
  }),
}))
vi.mock('@/client/generated', () => ({ resendVerificationEmail: vi.fn() }))
vi.mock('@/client/server-api', () => ({ createServerApiClient: vi.fn() }))
vi.mock('../navigation', () => ({ buildUpdateUserDetailsHref: vi.fn() }))

const userId = 42
const fakeClient = { fake: 'client' }
const run = () => resendWorkEmailVerification(userId, undefined, new FormData())
const mockApi = (result: unknown) =>
  vi.mocked(resendVerificationEmail).mockResolvedValue(result as never)

describe('resendWorkEmailVerification', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(createServerApiClient).mockResolvedValue(fakeClient as never)
    vi.mocked(buildUpdateUserDetailsHref).mockReturnValue(
      '/user/details?detail=email&emailLinkResent=true',
    )
  })

  it('calls the API with the user id and server client', async () => {
    mockApi({ response: { ok: false }, error: undefined })

    await run()

    expect(resendVerificationEmail).toHaveBeenCalledWith({ path: { userId }, client: fakeClient })
  })

  it('redirects with emailLinkResent when the API succeeds', async () => {
    mockApi({ response: { ok: true } })

    await expect(run()).rejects.toMatchObject({
      url: '/user/details?detail=email&emailLinkResent=true',
    })
    expect(buildUpdateUserDetailsHref).toHaveBeenCalledWith({
      detail: 'email',
      emailLinkResent: true,
    })
  })

  it('returns the API error title when the request fails', async () => {
    mockApi({ response: { ok: false }, error: { title: 'Too many requests' } })

    await expect(run()).resolves.toEqual({ status: 'error', message: 'Too many requests' })
  })

  it('returns a fallback message when the API error has no title', async () => {
    mockApi({ response: { ok: false }, error: undefined })

    await expect(run()).resolves.toEqual({
      status: 'error',
      message: 'An unexpected error occurred.',
    })
  })

  it('returns a fallback message when the request throws', async () => {
    vi.mocked(resendVerificationEmail).mockRejectedValue(new Error('network down'))

    await expect(run()).resolves.toEqual({
      status: 'error',
      message: 'An unexpected error occurred.',
    })
  })

  it('rethrows Next control-flow errors instead of swallowing them', async () => {
    const controlFlow = new NextControlFlowError('NEXT_REDIRECT')
    vi.mocked(resendVerificationEmail).mockRejectedValue(controlFlow)

    await expect(run()).rejects.toBe(controlFlow)
  })
})
