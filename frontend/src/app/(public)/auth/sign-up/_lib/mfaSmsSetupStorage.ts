import { createSessionStorageStore } from '@/lib/sessionStorageStore'

type SmsMfaSetupData = {
  phoneNumber: string
  authenticationSession: string
}

export const signUpSmsMfaSetupStorageKey = 'signUpSmsSetup:v1'

// Created once at module level so useSyncExternalStore receives stable
// subscribe/getSnapshot references and the snapshot cache survives re-renders.
const mfaSmsSetupStore = createSessionStorageStore<SmsMfaSetupData>(signUpSmsMfaSetupStorageKey)

export const useSmsMfaSetupStore = () => mfaSmsSetupStore.useStore()
