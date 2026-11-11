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

  // No challenge (expired after 10 mins, or the user landed here directly)
  if (!challenge) {
    return null
  }

  return JSON.parse(challenge) as MultiFactorAuthenticationSetupDto
}
