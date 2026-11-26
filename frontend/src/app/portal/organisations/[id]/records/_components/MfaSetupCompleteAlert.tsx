'use client'

import { useEffect, useState } from 'react'

import { Alert } from '@/components/Alert/Alert'
import { useMfaSetupCompleteStore } from '@/lib/auth/mfa/mfaSetupCompleteFlag'

// Clears the flag once read, so the alert shows on arrival but not after a refresh or a later
// visit. `shown` keeps the alert up after clearing flips the store back to empty.
export const MfaSetupCompleteAlert = () => {
  const { status, clear } = useMfaSetupCompleteStore()
  const [shown, setShown] = useState(false)

  if (status === 'ready' && !shown) setShown(true)

  useEffect(() => {
    if (status === 'ready') clear()
  }, [status, clear])

  if (!shown) return null

  return (
    <Alert type="success">
      <h3>Two-factor authentication setup complete</h3>
      <p>You can now access the UK PharmaScan system.</p>
    </Alert>
  )
}
