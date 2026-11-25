import { type SignUpMfaSetup, signUpMfaSetupSchema } from '@/lib/auth/mfa/SetupMfaForm'

import { createSessionStorageStore } from './sessionStorageStore'

export const signUpMfaSetupStorageKey = 'signUpMfaSetup:v1'

const isValidMfaSetup = (value: unknown): value is SignUpMfaSetup => {
  const result = signUpMfaSetupSchema.safeParse(value)
  if (!result.success) return false

  try {
    new URL(result.data.otpAuthUri)
    return true
  } catch {
    return false
  }
}

// Created once at module level so useSyncExternalStore receives stable
// subscribe/getSnapshot references and the snapshot cache survives re-renders.
const mfaSetupStore = createSessionStorageStore<SignUpMfaSetup>(signUpMfaSetupStorageKey, {
  validate: isValidMfaSetup,
})

export const useMfaSetupStore = () => mfaSetupStore.useStore()
