import { cookies } from 'next/headers'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { MultiFactorAuthenticationSetupDto } from '@/client/generated'
import { fakeMultiFactorAuthenticationSetupDto } from '@/client/generated/@faker-js/faker.gen'

import { getOtpCookieData, setOtpCookieData } from './mfaHelpers'

vi.mock('next/headers', () => ({
  cookies: vi.fn(),
}))

const mockData: MultiFactorAuthenticationSetupDto = fakeMultiFactorAuthenticationSetupDto()

describe('otp cookie helpers', () => {
  const cookieStore = {
    set: vi.fn(),
    get: vi.fn(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(cookies).mockResolvedValue(
      cookieStore as unknown as Awaited<ReturnType<typeof cookies>>,
    )
  })

  describe('setOtpCookieData', () => {
    it('stores the serialised data under the otp_challenge key', async () => {
      await setOtpCookieData(mockData)

      expect(cookieStore.set).toHaveBeenCalledTimes(1)
      expect(cookieStore.set).toHaveBeenCalledWith(
        'otp_challenge',
        JSON.stringify(mockData),
        expect.any(Object),
      )
    })

    it('sets secure cookie options with a 10 minute expiry', async () => {
      await setOtpCookieData(mockData)

      const [, , options] = cookieStore.set.mock.calls[0]
      expect(options).toEqual({
        httpOnly: true,
        secure: true,
        sameSite: 'lax',
        maxAge: 600,
      })
    })

    it('awaits the cookies() store before writing', async () => {
      await setOtpCookieData(mockData)

      expect(cookies).toHaveBeenCalledTimes(1)
    })
  })

  describe('getOtpCookieData', () => {
    it('reads the otp_challenge cookie', async () => {
      cookieStore.get.mockReturnValue({ value: JSON.stringify(mockData) })

      await getOtpCookieData()

      expect(cookieStore.get).toHaveBeenCalledWith('otp_challenge')
    })

    it('returns the parsed data when the cookie exists', async () => {
      cookieStore.get.mockReturnValue({ value: JSON.stringify(mockData) })

      await expect(getOtpCookieData()).resolves.toEqual(mockData)
    })

    it('returns null when the cookie does not exist', async () => {
      cookieStore.get.mockReturnValue(undefined)

      await expect(getOtpCookieData()).resolves.toBeNull()
    })

    it('returns null when the cookie value is an empty string', async () => {
      cookieStore.get.mockReturnValue({ value: '' })

      await expect(getOtpCookieData()).resolves.toBeNull()
    })

    it('throws when the cookie contains invalid JSON', async () => {
      cookieStore.get.mockReturnValue({ value: 'not-json{' })

      await expect(getOtpCookieData()).rejects.toThrow(SyntaxError)
    })
  })

  describe('round trip', () => {
    it('returns what was previously stored', async () => {
      let stored: string | undefined
      cookieStore.set.mockImplementation((_key: string, value: string) => {
        stored = value
      })
      cookieStore.get.mockImplementation(() =>
        stored === undefined ? undefined : { value: stored },
      )

      await setOtpCookieData(mockData)

      await expect(getOtpCookieData()).resolves.toEqual(mockData)
    })
  })
})
