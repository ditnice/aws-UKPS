import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { postAuthSetupUser } from '@/client/generated/sdk.gen'

import { signUpMfaSetupStorageKey } from '../../_lib/mfaSetupStorage'

import { SignUpSetPasswordForm } from './SignUpSetPasswordForm'

const mockPush = vi.fn()
const setupToken = 'test-setup-token'

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

vi.mock('@/client/generated/sdk.gen', () => ({
  postAuthSetupUser: vi.fn(),
}))

beforeEach(() => {
  vi.mocked(postAuthSetupUser).mockResolvedValue({
    data: {
      authenticationSession: 'test-authentication-session',
      otpAuthUri: 'otpauth://totp/test',
    },
    error: undefined,
  })
})

afterEach(() => {
  cleanup()
  sessionStorage.clear()
  vi.clearAllMocks()
})

function renderForm() {
  render(<SignUpSetPasswordForm setupToken={setupToken} />)
}

function enterPassword(password: string) {
  fireEvent.change(screen.getByLabelText('Password'), {
    target: { value: password },
  })
}

function submitForm() {
  fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
}

describe('SignUpSetPasswordForm', () => {
  it('renders the set password controls', () => {
    renderForm()

    expect(screen.getByText('Your password must:')).toBeDefined()
    expect(screen.getByText('be at least 8 characters long')).toBeDefined()
    expect(screen.getByLabelText('Password')).toBeDefined()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeDefined()
  })

  it('sets autocomplete for a new password', () => {
    renderForm()

    expect(screen.getByLabelText('Password').getAttribute('autocomplete')).toBe('new-password')
  })

  it('shows a required validation error when submitted empty', async () => {
    renderForm()

    submitForm()

    expect(await screen.findByText('Enter your password')).toBeDefined()
  })

  it('shows a minimum length validation error for a short password', async () => {
    renderForm()

    enterPassword('short')
    submitForm()

    expect(await screen.findByText('Password must be at least 8 characters long')).toBeDefined()
  })

  it('does not show validation errors for a valid password', async () => {
    renderForm()

    enterPassword('fourteen-chars')
    submitForm()

    await waitFor(() => {
      expect(screen.queryByText('Enter your password')).toBeNull()
      expect(screen.queryByText('Password must be at least 8 characters long')).toBeNull()
    })
  })

  it('revalidates the password on blur after a failed submit', async () => {
    renderForm()

    enterPassword('short')
    submitForm()

    expect(await screen.findByText('Password must be at least 8 characters long')).toBeDefined()

    enterPassword('fourteen-chars')
    fireEvent.blur(screen.getByLabelText('Password'))

    await waitFor(() => {
      expect(screen.queryByText('Password must be at least 8 characters long')).toBeNull()
    })
  })

  it('submits the setup token and password, stores MFA setup data, and redirects', async () => {
    renderForm()

    enterPassword('fourteen-chars')
    submitForm()

    await waitFor(() => {
      expect(postAuthSetupUser).toHaveBeenCalledWith({
        body: {
          newPassword: 'fourteen-chars',
          setupToken,
        },
        credentials: 'include',
      })
      expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBe(
        JSON.stringify({
          authenticationSession: 'test-authentication-session',
          otpAuthUri: 'otpauth://totp/test',
          setupToken,
        }),
      )
      expect(mockPush).toHaveBeenCalledWith('/auth/sign-up/set-mfa')
    })
  })

  it('shows a specific error if MFA setup data cannot be stored', async () => {
    const setItem = vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('Storage disabled')
    })
    renderForm()

    enterPassword('fourteen-chars')
    submitForm()

    expect(
      await screen.findByText(
        'Your password was created, but we could not continue to two-factor authentication setup. Return to your sign-up link and try again.',
      ),
    ).toBeDefined()
    expect(mockPush).not.toHaveBeenCalled()
    setItem.mockRestore()
  })

  it('shows a password error for a 400 response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: { status: 400 },
      response: new Response(null, { status: 400 }),
    })
    renderForm()

    enterPassword('fourteen-chars')
    submitForm()

    expect(
      await screen.findByText('The password does not meet the expected standards.'),
    ).toBeDefined()
    expect(mockPush).not.toHaveBeenCalled()
  })

  it('shows a setup link error for a 401 response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The setup token has expired and can no longer be used.',
        status: 401,
      },
      response: new Response(null, { status: 401 }),
    })
    renderForm()

    enterPassword('fourteen-chars')
    submitForm()

    expect(
      await screen.findByText('The setup token has expired and can no longer be used.'),
    ).toBeDefined()
    expect(mockPush).not.toHaveBeenCalled()
  })

  it('shows a setup link error for a 404 response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The supplied setup token does not exist.',
        status: 404,
      },
      response: new Response(null, { status: 404 }),
    })
    renderForm()

    enterPassword('fourteen-chars')
    submitForm()

    expect(await screen.findByText('The supplied setup token does not exist.')).toBeDefined()
    expect(mockPush).not.toHaveBeenCalled()
  })

  it('shows a generic error if password setup fails unexpectedly', async () => {
    vi.mocked(postAuthSetupUser).mockRejectedValue(new Error('Network error'))
    renderForm()

    enterPassword('fourteen-chars')
    submitForm()

    expect(
      await screen.findByText('We could not create your password. Try again later.'),
    ).toBeDefined()
    expect(mockPush).not.toHaveBeenCalled()
  })
})
