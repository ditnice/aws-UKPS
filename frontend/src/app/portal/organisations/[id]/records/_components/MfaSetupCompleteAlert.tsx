'use client'

import { useEffect, useState, useSyncExternalStore } from 'react'

import { Alert } from '@/components/Alert/Alert'
import { clearMfaSetupComplete, isMfaSetupComplete } from '@/lib/auth/mfa/mfaSetupCompleteFlag'

const subscribe = () => () => undefined

// Session storage is only readable in the browser, so nothing renders on the server.
export const MfaSetupCompleteAlert = () => {
  const isClient = useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  )

  return isClient ? <MfaSetupCompleteAlertOnce /> : null
}

// Reads the flag once on mount and clears it, so the alert shows on arrival but not after a
// refresh or a later visit.
const MfaSetupCompleteAlertOnce = () => {
  const [show] = useState(isMfaSetupComplete)

  useEffect(() => {
    clearMfaSetupComplete()
  }, [])

  if (!show) return null

  return (
    <Alert type="success">
      <h3>Two-factor authentication setup complete</h3>
      <p>You can now access the UK PharmaScan system.</p>
    </Alert>
  )
}
