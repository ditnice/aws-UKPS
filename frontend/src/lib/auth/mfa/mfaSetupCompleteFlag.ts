import { createSessionStorageStore } from '@/lib/sessionStorageStore'

// Set when sign-up MFA setup succeeds so the records page, reached after the organisation picker,
// can confirm it once. Session storage because the organisation is not known at sign-up.
export const mfaSetupCompleteStorageKey = 'mfaSetupComplete:v1'

const mfaSetupCompleteStore = createSessionStorageStore<true>(mfaSetupCompleteStorageKey, {
  validate: (value): value is true => value === true,
})

export const markMfaSetupComplete = () => mfaSetupCompleteStore.set(true)

export const useMfaSetupCompleteStore = () => mfaSetupCompleteStore.useStore()
