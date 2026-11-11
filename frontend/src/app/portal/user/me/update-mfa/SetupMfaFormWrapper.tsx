'use client'

import { redirect } from 'next/navigation'

import { MultiFactorAuthenticationSetupDto } from '@/client/generated'
import SetupMfaForm from '@/lib/auth/mfa/SetupMfaForm'

import { clearOtpCookieData } from './mfaHelpers'

type SetupMfaFormWrapperProps = {
  redirectOnSuccess: string
  setup: MultiFactorAuthenticationSetupDto
}
const SetupMfaFormWrapper = ({ setup, redirectOnSuccess }: SetupMfaFormWrapperProps) => {
  const completeMfaSetup = (redirectOnSuccess: string) => {
    'use server'
    clearOtpCookieData()
    redirect(redirectOnSuccess)
  }
  return (
    <SetupMfaForm
      setup={{ type: 'update', ...setup }}
      hideAlternativeOptions={true}
      onSuccess={async () => {
        await completeMfaSetup(redirectOnSuccess)
      }}
    />
  )
}

export default SetupMfaFormWrapper
