'use client'

import { useRouter } from 'next/navigation'
import { useSyncExternalStore } from 'react'

import { PageHeader } from '@/components/PageHeader/PageHeader'
import SetupMfaForm, { SignUpMfaSetup, signUpMfaSetupSchema } from '@/lib/auth/mfa/SetupMfaForm'
import { routeOnSuccessfulAuth } from '@/lib/auth/routing'

import { signUpMfaSetupStorageKey } from '../_lib/mfaSetupStorage'

type SetupState =
  { setup: SignUpMfaSetup; status: 'ready' } | { status: 'error' } | { status: 'loading' }

const loadingSetupState: SetupState = { status: 'loading' }
const errorSetupState: SetupState = { status: 'error' }
let cachedStoredSetup: null | string | undefined
let cachedSetupState: SetupState | undefined

function loadSignUpMfaSetup(): SetupState {
  let storedSetup: string | null

  try {
    storedSetup = sessionStorage.getItem(signUpMfaSetupStorageKey)
  } catch {
    return errorSetupState
  }

  if (cachedSetupState && storedSetup === cachedStoredSetup) return cachedSetupState
  cachedStoredSetup = storedSetup

  if (!storedSetup) return (cachedSetupState = errorSetupState)

  try {
    const parsedSetup = signUpMfaSetupSchema.parse(JSON.parse(storedSetup))
    new URL(parsedSetup.otpAuthUri)

    return (cachedSetupState = { setup: parsedSetup, status: 'ready' })
  } catch {
    return (cachedSetupState = errorSetupState)
  }
}

function subscribeToSetupStorage() {
  return () => {}
}

export default function SignUpSetMfa() {
  const router = useRouter()
  const setupState = useSyncExternalStore(
    subscribeToSetupStorage,
    loadSignUpMfaSetup,
    () => loadingSetupState,
  )

  if (setupState.status === 'loading') return null

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

  const { setup } = setupState

  return (
    <>
      <PageHeader heading="Set up two-factor authentication"></PageHeader>
      <p>
        We use two-factor authentication to help protect your account. Scan the QR code using your
        chosen authenticator.
      </p>
      <SetupMfaForm
        setup={{ type: 'initialise', ...setup }}
        onSuccess={() => {
          try {
            sessionStorage.removeItem(signUpMfaSetupStorageKey)
          } catch {}

          router.push(routeOnSuccessfulAuth)
        }}
      />
    </>
  )
}
