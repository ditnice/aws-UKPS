'use client'

import { MultiFactorAuthenticationSetupDto } from '@/client/generated'
import SetupMfaForm from '@/lib/auth/mfa/SetupMfaForm'

import { completeMfaSetup } from './serverActions'

type SetupMfaFormWrapperProps = {
  redirectOnSuccess: string
  setup: MultiFactorAuthenticationSetupDto
}
const SetupMfaFormWrapper = ({ setup, redirectOnSuccess }: SetupMfaFormWrapperProps) => {
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
