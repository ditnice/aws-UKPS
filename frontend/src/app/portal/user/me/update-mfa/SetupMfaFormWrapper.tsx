'use client'

import { useRouter } from 'next/navigation'

import { MultiFactorAuthenticationSetupDto } from '@/client/generated'
import SetupMfaForm from '@/lib/auth/mfa/SetupMfaForm'

type SetupMfaFormWrapperProps = {
  redirectOnSuccess: string
  setup: MultiFactorAuthenticationSetupDto
}
const SetupMfaFormWrapper = ({ setup, redirectOnSuccess }: SetupMfaFormWrapperProps) => {
  const router = useRouter()
  return (
    <SetupMfaForm
      setup={{ type: 'update', ...setup }}
      hideAlternativeOptions={true}
      onSuccess={() => {
        router.replace(redirectOnSuccess)
      }}
    />
  )
}

export default SetupMfaFormWrapper
