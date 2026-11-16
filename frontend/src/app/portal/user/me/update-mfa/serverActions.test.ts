import { redirect } from 'next/navigation'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { initiateUpdateMultiFactorAuthentication } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import { setOtpCookieData } from './mfaHelpers'
import { generateOtp } from './serverActions'

vi.mock('@/client/generated', () => ({
  initiateUpdateMultiFactorAuthentication: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('./mfaHelpers', () => ({
  setOtpCookieData: vi.fn(),
}))

// The real redirect() throws a NEXT_REDIRECT error to halt execution.
// Mirroring that matters: otherwise code after a redirect would keep running.
vi.mock('next/navigation', () => ({
  redirect: vi.fn((url: string) => {
    throw new Error(`NEXT_REDIRECT:${url}`)
  }),
}))

const mockClient = { name: 'mock-api-client' }
const mockOtpData = { secret: 'ABC123', uri: 'otpauth://totp/test' }

describe('generateOtp', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(createServerApiClient).mockResolvedValue(mockClient as never)
  })

  it('calls the API with the server api client', async () => {
    vi.mocked(initiateUpdateMultiFactorAuthentication).mockResolvedValue({
      data: mockOtpData,
      error: undefined,
    } as never)

    await expect(generateOtp()).rejects.toThrow()

    expect(createServerApiClient).toHaveBeenCalledTimes(1)
    expect(initiateUpdateMultiFactorAuthentication).toHaveBeenCalledWith({
      client: mockClient,
    })
  })

  describe('when the API call succeeds', () => {
    beforeEach(() => {
      vi.mocked(initiateUpdateMultiFactorAuthentication).mockResolvedValue({
        data: mockOtpData,
        error: undefined,
      } as never)
    })

    it('stores the OTP data in the cookie', async () => {
      await expect(generateOtp()).rejects.toThrow()

      expect(setOtpCookieData).toHaveBeenCalledTimes(1)
      expect(setOtpCookieData).toHaveBeenCalledWith(mockOtpData)
    })

    it('redirects to the verify step', async () => {
      await expect(generateOtp()).rejects.toThrow('NEXT_REDIRECT:?verify=verify')

      expect(redirect).toHaveBeenCalledTimes(1)
      expect(redirect).toHaveBeenCalledWith('?verify=verify')
    })

    it('sets the cookie before redirecting', async () => {
      await expect(generateOtp()).rejects.toThrow()

      const cookieOrder = vi.mocked(setOtpCookieData).mock.invocationCallOrder[0]
      const redirectOrder = vi.mocked(redirect).mock.invocationCallOrder[0]
      expect(cookieOrder).toBeLessThan(redirectOrder)
    })
  })

  describe('when the API call fails', () => {
    it('redirects with an error when no data is returned', async () => {
      vi.mocked(initiateUpdateMultiFactorAuthentication).mockResolvedValue({
        data: undefined,
        error: undefined,
      } as never)

      await expect(generateOtp()).rejects.toThrow('NEXT_REDIRECT:?error=generate-failed')

      expect(redirect).toHaveBeenCalledWith('?error=generate-failed')
      expect(setOtpCookieData).not.toHaveBeenCalled()
    })

    it('redirects with an error when an error is returned', async () => {
      vi.mocked(initiateUpdateMultiFactorAuthentication).mockResolvedValue({
        data: undefined,
        error: { message: 'Something went wrong' },
      } as never)

      await expect(generateOtp()).rejects.toThrow('NEXT_REDIRECT:?error=generate-failed')

      expect(setOtpCookieData).not.toHaveBeenCalled()
    })

    it('treats a response with both data and error as a failure', async () => {
      vi.mocked(initiateUpdateMultiFactorAuthentication).mockResolvedValue({
        data: mockOtpData,
        error: { message: 'Partial failure' },
      } as never)

      await expect(generateOtp()).rejects.toThrow('NEXT_REDIRECT:?error=generate-failed')

      expect(setOtpCookieData).not.toHaveBeenCalled()
    })
  })

  describe('unexpected failures', () => {
    it('propagates errors thrown while creating the API client', async () => {
      vi.mocked(createServerApiClient).mockRejectedValue(new Error('client failed'))

      await expect(generateOtp()).rejects.toThrow('client failed')

      expect(initiateUpdateMultiFactorAuthentication).not.toHaveBeenCalled()
      expect(redirect).not.toHaveBeenCalled()
    })

    it('propagates errors thrown while setting the cookie, without redirecting', async () => {
      vi.mocked(initiateUpdateMultiFactorAuthentication).mockResolvedValue({
        data: mockOtpData,
        error: undefined,
      } as never)
      vi.mocked(setOtpCookieData).mockRejectedValue(new Error('cookie failed'))

      await expect(generateOtp()).rejects.toThrow('cookie failed')

      expect(redirect).not.toHaveBeenCalled()
    })
  })
})
