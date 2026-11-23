import { cookies } from 'next/headers'

import { MultiFactorAuthenticationUpdateDto } from '@/client/generated'

const otpChallengeKey = 'otp_challenge'

export const setOtpCookieData = async (data: MultiFactorAuthenticationUpdateDto) => {
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

  return JSON.parse(challenge) as MultiFactorAuthenticationUpdateDto
}

export const clearOtpCookieData = async () => {
  const cookieCollection = await cookies()
  cookieCollection.delete(otpChallengeKey)
}
