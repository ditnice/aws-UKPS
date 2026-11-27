'use client'

import { useState } from 'react'

import { buildPortalRecordActionHref } from '@/app/portal/organisations/[id]/records/_lib/recordActionAlert'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import SetupMfaForm from '@/lib/auth/mfa/SetupMfaForm'
import { navigateOnSuccessfulAuth } from '@/lib/auth/routing'

import { useMfaSetupStore } from '../_lib/mfaSetupStorage'
import { useSignUpPassword } from '../_lib/SignUpPasswordContext'

export default function SignUpSetMfa() {
  const { clearPassword } = useSignUpPassword()
  const setupState = useMfaSetupStore()

  const [success, setSuccess] = useState(false)

  if (setupState.status === 'loading' || success) return null

  if (setupState.status === 'error') {
    return (
      <>
        <PageHeader heading="There is a problem setting up two-factor authentication"></PageHeader>
        <p>
          We could not find your multi-factor authentication setup details. Return to your sign-up
          link and try again.
        </p>
      </>
    )
  }

  const { data: setup, clear: clearSetup } = setupState

  return (
    <>
      <PageHeader heading="Set up two-factor authentication"></PageHeader>
      <p>
        We use two-factor authentication to help protect your account. Scan the QR code using your
        chosen authenticator.
      </p>
      <SetupMfaForm
        setup={{ type: 'initialise', ...setup }}
        alternativeHref="/auth/sign-up/set-mfa/sms/enter-phone-number"
        onSuccess={() => {
          setSuccess(true)
          clearSetup()
          clearPassword()
          // Through the portal root, which knows the organisation, to show the success banner
          navigateOnSuccessfulAuth(buildPortalRecordActionHref('mfa-setup-complete'))
        }}
      />
    </>
  )
}
