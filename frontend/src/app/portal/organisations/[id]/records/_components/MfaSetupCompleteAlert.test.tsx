import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import {
  markMfaSetupComplete,
  mfaSetupCompleteStorageKey,
} from '@/lib/auth/mfa/mfaSetupCompleteFlag'

import { MfaSetupCompleteAlert } from './MfaSetupCompleteAlert'

afterEach(() => {
  cleanup()
  sessionStorage.clear()
})

const heading = 'Two-factor authentication setup complete'

describe('MfaSetupCompleteAlert', () => {
  it('confirms MFA setup when it has just completed', () => {
    markMfaSetupComplete()

    render(<MfaSetupCompleteAlert />)

    expect(screen.getByRole('heading', { level: 3, name: heading })).toBeTruthy()
    expect(screen.getByText('You can now access the UK PharmaScan system.')).toBeTruthy()
  })

  it('shows the confirmation only once', () => {
    markMfaSetupComplete()

    const { unmount } = render(<MfaSetupCompleteAlert />)
    expect(sessionStorage.getItem(mfaSetupCompleteStorageKey)).toBeNull()
    expect(screen.getByRole('heading', { name: heading })).toBeTruthy()

    unmount()
    render(<MfaSetupCompleteAlert />)

    expect(screen.queryByRole('heading', { name: heading })).toBeNull()
  })

  it('renders nothing when MFA setup has not just completed', () => {
    const { container } = render(<MfaSetupCompleteAlert />)

    expect(container).toBeEmptyDOMElement()
  })
})
