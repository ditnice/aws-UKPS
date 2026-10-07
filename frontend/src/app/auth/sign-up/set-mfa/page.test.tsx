import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { postAuthVerifyMfa } from '@/client/generated/sdk.gen'
import { routeOnSuccessfulAuth } from '@/lib/auth/routing'
import { errorMessages } from '@/lib/form/errorMessages'
import { fillInput } from '@/test-utils/userInteractions'

import { signUpMfaSetupStorageKey } from '../_lib/mfaSetupStorage'

import SignUpSetMfa from './page'

const mockPush = vi.fn()

const setup = {
  authenticationSession: 'test-authentication-session',
  otpAuthUri:
    'otpauth://totp/UK%20PharmaScan:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=UK%20PharmaScan&algorithm=SHA1&digits=6&period=30',
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
  sessionStorage.clear()
})

function renderPage() {
  render(<SignUpSetMfa />)
}

async function enterSecurityCode(securityCode: string) {
  await fillInput(user, screen.getByLabelText('Enter your authentication code'), securityCode)
}

async function submitForm() {
  await user.click(screen.getByRole('button', { name: 'Continue' }))
}

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('SignUpSetMfa', () => {
  it('renders an error if setup details are missing', async () => {
    sessionStorage.removeItem(signUpMfaSetupStorageKey)

    renderPage()

    expect(
      await screen.findByRole('heading', {
        name: 'There is a problem setting up two-factor authentication',
      }),
    ).toBeInTheDocument()
    expect(
      screen.getByText(
        'We could not find your multi-factor authentication setup details. Return to your sign-up link and try again.',
      ),
    ).toBeInTheDocument()
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
    ).toBeInTheDocument()
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
    ).toBeInTheDocument()
    getItem.mockRestore()
  })

  it('renders the MFA setup controls', async () => {
    renderPage()

    expect(
      await screen.findByRole('heading', { name: 'Set up two-factor authentication' }),
    ).toBeInTheDocument()
    expect(screen.getByLabelText('QR code for authenticator app setup')).toBeInTheDocument()
    expect(screen.getByText('JBSWY3DPEHPK3PXP')).toBeInTheDocument()
    expect(screen.getByLabelText('Enter your authentication code')).toBeInTheDocument()
    expect(
      screen.getByText('Enter the 6-digit authentication code shown in your authenticator app.'),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeInTheDocument()
  })

  it('sets autocomplete for a one-time code', async () => {
    renderPage()

    expect(
      (await screen.findByLabelText('Enter your authentication code')).getAttribute('autocomplete'),
    ).toBe('one-time-code')
  })

  it('shows a required validation error when submitted empty', async () => {
    renderPage()

    await submitForm()

    expect(await screen.findByText('Enter your security code')).toBeInTheDocument()
  })

  it('shows a format validation error for an invalid code', async () => {
    renderPage()

    await enterSecurityCode('12345')
    await submitForm()

    expect(await screen.findByText('Enter a 6-digit security code')).toBeInTheDocument()
  })

  it('does not show validation errors for a valid code', async () => {
    renderPage()

    await enterSecurityCode('123456')
    await submitForm()

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })

  it('submits grouped security codes using the normalised code', async () => {
    renderPage()

    await enterSecurityCode('12 324-6')
    await submitForm()

    await waitFor(() => {
      expect(postAuthVerifyMfa).toHaveBeenCalledWith({
        body: {
          authenticationSession: setup.authenticationSession,
          code: '123246',
          setupToken: setup.setupToken,
        },
        credentials: 'include',
      })
    })
  })

  it('revalidates fields on blur after a failed submit', async () => {
    renderPage()

    await submitForm()

    expect(await screen.findByText('Enter your security code')).toBeInTheDocument()

    await enterSecurityCode('123 456')
    await user.tab()

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })

  it('clears setup details and redirects to the portal after successful verification', async () => {
    renderPage()

    await enterSecurityCode('123456')
    await submitForm()

    await waitFor(() => {
      expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
      expect(mockPush).toHaveBeenCalledWith(routeOnSuccessfulAuth)
    })
  })

  it('shows an invalid code error for a 400 response', async () => {
    vi.mocked(postAuthVerifyMfa).mockResolvedValue({
      data: undefined,
      error: { status: 400 },
      response: new Response(null, { status: 400 }),
    })
    renderPage()

    await enterSecurityCode('123456')
    await submitForm()

    expect(await screen.findByText(errorMessages.incorrectMfaCode)).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
  })

  it('shows the backend detail for a 500 response without clearing setup details', async () => {
    vi.mocked(postAuthVerifyMfa).mockResolvedValue({
      data: undefined,
      error: { status: 500, detail: 'The service is temporarily unavailable.' },
      response: new Response(null, { status: 500 }),
    })
    renderPage()
    await enterSecurityCode('123456')
    await submitForm()

    expect(await screen.findByText('The service is temporarily unavailable.')).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBe(JSON.stringify(setup))
  })

  it('shows the generic error for a 500 response without detail', async () => {
    vi.mocked(postAuthVerifyMfa).mockResolvedValue({
      data: undefined,
      error: { status: 500 },
      response: new Response(null, { status: 500 }),
    })
    renderPage()
    await enterSecurityCode('123456')
    await submitForm()

    expect(
      await screen.findByText('We could not verify your authentication code. Try again later.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBe(JSON.stringify(setup))
  })

  it('shows the generic error when the API error has no response', async () => {
    vi.mocked(postAuthVerifyMfa).mockResolvedValue({
      data: undefined,
      error: { status: 500 },
    })
    renderPage()
    await enterSecurityCode('123456')
    await submitForm()

    expect(
      await screen.findByText('We could not verify your authentication code. Try again later.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBe(JSON.stringify(setup))
  })

  it('shows a generic error if verification fails unexpectedly', async () => {
    vi.mocked(postAuthVerifyMfa).mockRejectedValue(new Error('Network error'))
    renderPage()

    await enterSecurityCode('123456')
    await submitForm()

    expect(
      await screen.findByText('We could not verify your authentication code. Try again later.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
  })
})
