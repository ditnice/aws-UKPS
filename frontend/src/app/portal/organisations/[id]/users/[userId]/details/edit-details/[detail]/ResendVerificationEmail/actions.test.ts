import { beforeEach, describe, expect, it, vi } from 'vitest'

import { resendVerificationEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import { resendWorkEmailVerification } from './actions' // adjust to your file name

class RedirectError extends Error {
  constructor(public url: string) {
    super('NEXT_REDIRECT')
  }
}

vi.mock('next/navigation', () => ({
  redirect: vi.fn((url: string) => {
    throw new RedirectError(url)
  }),
  // Mirror the real behaviour: framework errors are rethrown, others are swallowed
  unstable_rethrow: vi.fn((error: unknown) => {
    if (error instanceof RedirectError || (error as Error)?.message === 'DYNAMIC_SERVER_USAGE') {
      throw error
    }
  }),
}))

vi.mock('@/client/generated', () => ({ resendVerificationEmail: vi.fn() }))
vi.mock('@/client/server-api', () => ({ createServerApiClient: vi.fn() }))
const navigation = { onSuccess: '/resend-success', onError: '/resend-error' }

const run = () => resendWorkEmailVerification(navigation, 42, undefined, new FormData())

describe('resendWorkEmailVerification', () => {
  const client = { name: 'api-client' }

  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(createServerApiClient).mockResolvedValue(client as never)
  })

  it('calls the API with the user id and server client', async () => {
    vi.mocked(resendVerificationEmail).mockResolvedValue({ response: { ok: true } } as never)

    await expect(run()).rejects.toBeInstanceOf(RedirectError)

    expect(resendVerificationEmail).toHaveBeenCalledWith({ path: { userId: 42 }, client })
  })

  it('redirects to the success navigation when the request succeeds', async () => {
    vi.mocked(resendVerificationEmail).mockResolvedValue({ response: { ok: true } } as never)

    await expect(run()).rejects.toMatchObject({ url: navigation.onSuccess })
  })

  it('redirects to the error navigation when the response is not ok', async () => {
    vi.mocked(resendVerificationEmail).mockResolvedValue({
      response: { ok: false },
      error: { message: 'nope' },
    } as never)

    await expect(run()).rejects.toMatchObject({ url: navigation.onError })
  })

  it('redirects to the error navigation when the API call throws', async () => {
    vi.mocked(resendVerificationEmail).mockRejectedValue(new Error('network down'))

    await expect(run()).rejects.toMatchObject({ url: navigation.onError })
  })

  it('rethrows framework errors instead of redirecting to the error page', async () => {
    const frameworkError = new Error('DYNAMIC_SERVER_USAGE')
    vi.mocked(createServerApiClient).mockRejectedValue(frameworkError)

    await expect(run()).rejects.toBe(frameworkError)
  })
})
