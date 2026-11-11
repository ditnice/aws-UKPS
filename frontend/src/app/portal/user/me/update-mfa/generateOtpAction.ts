'use server'

import { redirect } from 'next/navigation'

import { initiateUpdateMultiFactorAuthentication } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import { setOtpCookieData } from './mfaHelpers'

export const generateOtp = async () => {
  const { data, error } = await initiateUpdateMultiFactorAuthentication({
    client: await createServerApiClient(),
  })
  if (!data || error) {
    redirect(`?error=generate-failed`)
  }

  await setOtpCookieData(data)
  redirect(`?verify=verify`)
}
