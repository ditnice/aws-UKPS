import { cookies } from 'next/headers'

import { MultiFactorAuthenticationSetupDto } from '@/client/generated'

const otpChallengeKey = 'otp_challenge'

export const setOtpCookieData = async (data: MultiFactorAuthenticationSetupDto) => {
  const cookieCollection = await cookies()
  cookieCollection.set(otpChallengeKey, JSON.stringify(data), {
    httpOnly: true,
    secure: true,
    sameSite: 'lax',
    maxAge: 60 * 10, // 10 minutes
  })
}

export const getOtpCookieData = async () => {
  const cookieCollection = await cookies()
  const challenge = cookieCollection.get(otpChallengeKey)?.value

  if (!challenge) {
    return null
  }

  return JSON.parse(challenge) as MultiFactorAuthenticationSetupDto
}

export const clearOtpCookieData = async () => {
  const cookieCollection = await cookies()
  cookieCollection.delete(otpChallengeKey)
}
