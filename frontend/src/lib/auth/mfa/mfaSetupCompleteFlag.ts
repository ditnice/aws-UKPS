// Set when sign-up MFA setup succeeds so the records page, reached after the organisation picker,
// can confirm it once. Session storage because the organisation is not known at sign-up.
export const mfaSetupCompleteStorageKey = 'mfaSetupComplete:v1'

export function markMfaSetupComplete(): void {
  try {
    sessionStorage.setItem(mfaSetupCompleteStorageKey, 'true')
  } catch {
    // Storage blocked: the confirmation is skipped.
  }
}

export function isMfaSetupComplete(): boolean {
  try {
    return sessionStorage.getItem(mfaSetupCompleteStorageKey) === 'true'
  } catch {
    return false
  }
}

export function clearMfaSetupComplete(): void {
  try {
    sessionStorage.removeItem(mfaSetupCompleteStorageKey)
  } catch {
    // Nothing to clear when storage is blocked.
  }
}
