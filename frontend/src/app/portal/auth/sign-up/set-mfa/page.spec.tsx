import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { postAuthVerifyMfa } from '@/client/generated/sdk.gen'

import { signUpMfaSetupStorageKey } from '../constants'

import SignUpSetMfa from './page'

const mockPush = vi.fn()

const setup = {
  authenticationSession: 'test-authentication-session',
  otpAuthUri:
    'otpauth://totp/NICE%20UKPS:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=NICE%20UKPS&algorithm=SHA1&digits=6&period=30',
  setupToken: 'test-setup-token',
}

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

vi.mock('@/client/generated/sdk.gen', () => ({
  postAuthVerifyMfa: vi.fn(),
}))

beforeEach(() => {
  sessionStorage.setItem(signUpMfaSetupStorageKey, JSON.stringify(setup))
  vi.mocked(postAuthVerifyMfa).mockResolvedValue({
    data: undefined,
    error: undefined,
  })
})

afterEach(() => {
  cleanup()
  sessionStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(<SignUpSetMfa />)
}

function enterSecurityCode(securityCode: string) {
  fireEvent.change(screen.getByLabelText('Authentication code'), {
    target: { value: securityCode },
  })
}

function submitForm() {
  fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
}

describe('SignUpSetMfa', () => {
  it('renders an error if setup details are missing', async () => {
    sessionStorage.removeItem(signUpMfaSetupStorageKey)

    renderPage()

    expect(
      await screen.findByRole('heading', {
        name: 'There is a problem setting up two-factor authentication',
      }),
    ).toBeDefined()
    expect(
      screen.getByText(
        'We could not find your multi-factor authentication setup details. Return to your sign-up link and try again.',
      ),
    ).toBeDefined()
  })

  it('renders an error if setup details are invalid', async () => {
    sessionStorage.setItem(
      signUpMfaSetupStorageKey,
      JSON.stringify({ setupToken: 'test-setup-token' }),
    )

    renderPage()

    expect(
      await screen.findByRole('heading', {
        name: 'There is a problem setting up two-factor authentication',
      }),
    ).toBeDefined()
  })

  it('renders an error if setup details cannot be read from storage', async () => {
    const getItem = vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('Storage disabled')
    })

    renderPage()

    expect(
      await screen.findByRole('heading', {
        name: 'There is a problem setting up two-factor authentication',
      }),
    ).toBeDefined()
    getItem.mockRestore()
  })

  it('renders the MFA setup controls', async () => {
    renderPage()

    expect(
      await screen.findByRole('heading', { name: 'Set up two-factor authentication' }),
    ).toBeDefined()
    expect(screen.getByLabelText('QR code for authenticator app setup')).toBeDefined()
    expect(screen.getByText('JBSWY3DPEHPK3PXP')).toBeDefined()
    expect(screen.getByLabelText('Authentication code')).toBeDefined()
    expect(
      screen.getByText('Enter the 6-digit authentication code shown in your authenticator app.'),
    ).toBeDefined()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeDefined()
  })

  it('sets autocomplete for a one-time code', async () => {
    renderPage()

    expect((await screen.findByLabelText('Authentication code')).getAttribute('autocomplete')).toBe(
      'one-time-code',
    )
  })

  it('shows a required validation error when submitted empty', async () => {
    renderPage()

    submitForm()

    expect(await screen.findByText('Enter your security code')).toBeDefined()
  })

  it('shows a format validation error for an invalid code', async () => {
    renderPage()

    enterSecurityCode('12345')
    submitForm()

    expect(await screen.findByText('Enter a 6-digit security code')).toBeDefined()
  })

  it('does not show validation errors for a valid code', async () => {
    renderPage()

    enterSecurityCode('123456')
    submitForm()

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })

  it('submits grouped security codes using the normalised code', async () => {
    renderPage()

    enterSecurityCode('12 324-6')
    submitForm()

    await waitFor(() => {
      expect(postAuthVerifyMfa).toHaveBeenCalledWith({
        body: {
          authenticationSession: setup.authenticationSession,
          code: '123246',
          setupToken: setup.setupToken,
        },
      })
    })
  })

  it('revalidates fields on blur after a failed submit', async () => {
    renderPage()

    submitForm()

    expect(await screen.findByText('Enter your security code')).toBeDefined()

    enterSecurityCode('123 456')
    fireEvent.blur(screen.getByLabelText('Authentication code'))

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })

  it('clears setup details and redirects to sign in after successful verification', async () => {
    renderPage()

    enterSecurityCode('123456')
    submitForm()

    await waitFor(() => {
      expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
      expect(mockPush).toHaveBeenCalledWith('/portal/auth/sign-in')
    })
  })

  it('shows an invalid code error for a 400 response', async () => {
    vi.mocked(postAuthVerifyMfa).mockResolvedValue({
      data: undefined,
      error: { status: 400 },
      response: new Response(null, { status: 400 }),
    })
    renderPage()

    enterSecurityCode('123456')
    submitForm()

    expect(await screen.findByText('Invalid authentication code.')).toBeDefined()
    expect(mockPush).not.toHaveBeenCalled()
  })

  it('shows a generic error if verification fails unexpectedly', async () => {
    vi.mocked(postAuthVerifyMfa).mockRejectedValue(new Error('Network error'))
    renderPage()

    enterSecurityCode('123456')
    submitForm()

    expect(
      await screen.findByText('We could not verify your authentication code. Try again later.'),
    ).toBeDefined()
    expect(mockPush).not.toHaveBeenCalled()
  })
})
