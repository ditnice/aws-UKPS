import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { postAuthSetupUser } from '@/client/generated/sdk.gen'
import { errorMessages } from '@/lib/form/errorMessages'
import { fillInput } from '@/test-utils/userInteractions'

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
  vi.restoreAllMocks()
  sessionStorage.clear()
})

function renderForm() {
  render(<SignUpSetPasswordForm setupToken={setupToken} />)
}

async function enterPassword(password: string) {
  await fillInput(user, screen.getByLabelText('Password'), password)
}

async function submitForm() {
  await user.click(screen.getByRole('button', { name: 'Continue' }))
}

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('SignUpSetPasswordForm', () => {
  it('renders the set password controls', () => {
    renderForm()

    expect(screen.getByText('Your password must:')).toBeInTheDocument()
    expect(screen.getByText('be at least 8 characters long')).toBeInTheDocument()
    expect(screen.getByText('be 256 characters or fewer')).toBeInTheDocument()
    expect(screen.getByText('not contain spaces or other whitespace')).toBeInTheDocument()
    expect(screen.getByLabelText('Password')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeInTheDocument()
  })

  it('sets autocomplete for a new password', () => {
    renderForm()

    expect(screen.getByLabelText('Password').getAttribute('autocomplete')).toBe('new-password')
  })

  it('shows a required validation error when submitted empty', async () => {
    renderForm()

    await submitForm()

    expect(await screen.findByText('Enter your password')).toBeInTheDocument()
  })

  it('shows a minimum length validation error for a short password', async () => {
    renderForm()

    await enterPassword('1234567')
    await submitForm()

    expect(
      await screen.findByText('Password must be at least 8 characters long'),
    ).toBeInTheDocument()
    expect(postAuthSetupUser).not.toHaveBeenCalled()
  })

  it('accepts a password at the minimum length', async () => {
    renderForm()

    await enterPassword('12345678')
    await submitForm()

    await waitFor(() => {
      expect(screen.queryByText('Enter your password')).toBeNull()
      expect(screen.queryByText('Password must be at least 8 characters long')).toBeNull()
      expect(postAuthSetupUser).toHaveBeenCalledOnce()
    })
  })

  it('accepts a password at the maximum length', async () => {
    renderForm()

    await enterPassword('a'.repeat(256))
    await submitForm()

    await waitFor(() => {
      expect(postAuthSetupUser).toHaveBeenCalledOnce()
    })
  })

  it('shows a maximum length validation error for a password over 256 characters', async () => {
    renderForm()

    await enterPassword('a'.repeat(257))
    await submitForm()

    expect(await screen.findByText(errorMessages.passwordTooLong)).toBeInTheDocument()
    expect(postAuthSetupUser).not.toHaveBeenCalled()
  })

  it.each([
    ['leading space', ' password'],
    ['trailing space', 'password '],
    ['embedded space', 'pass word'],
    ['tab', 'pass\tword'],
  ])('rejects a password containing a %s', async (_description, password) => {
    renderForm()

    await enterPassword(password)
    await submitForm()

    expect(await screen.findByText(errorMessages.passwordWhitespace)).toBeInTheDocument()
    expect(postAuthSetupUser).not.toHaveBeenCalled()
  })

  it('accepts special characters', async () => {
    renderForm()

    await enterPassword('^$*.[]{}')
    await submitForm()

    await waitFor(() => {
      expect(postAuthSetupUser).toHaveBeenCalledOnce()
    })
  })

  it('revalidates the password on blur after a failed submit', async () => {
    renderForm()

    await enterPassword('short')
    await submitForm()

    expect(
      await screen.findByText('Password must be at least 8 characters long'),
    ).toBeInTheDocument()

    await enterPassword('fourteen-chars')
    await user.tab()

    await waitFor(() => {
      expect(screen.queryByText('Password must be at least 8 characters long')).toBeNull()
    })
  })

  it('submits the setup token and password, stores MFA setup data, and redirects', async () => {
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

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
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('Storage disabled')
    })
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(
      await screen.findByText(
        'Your password was created, but we could not continue to two-factor authentication setup. Return to your sign-up link and try again.',
      ),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })

  it('shows a password error for a 400 response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: { status: 400 },
      response: new Response(null, { status: 400 }),
    })
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(
      await screen.findByText('The password does not meet the expected standards.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })

  it('shows a setup link error for a 410 response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The setup token has expired and can no longer be used.',
        status: 410,
      },
      response: new Response(null, { status: 410 }),
    })
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(
      await screen.findByText('The setup token has expired and can no longer be used.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })

  it('shows a setup link error for a 409 response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The setup token has already been consumed and cannot be used again.',
        status: 409,
      },
      response: new Response(null, { status: 409 }),
    })
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(
      await screen.findByText(
        'The setup token has already been consumed and cannot be used again.',
      ),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
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

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(await screen.findByText('The supplied setup token does not exist.')).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })

  it.each([
    [410, 'This sign-up link has expired.'],
    [409, 'This sign-up link has already been used.'],
    [404, 'This sign-up link could not be found.'],
    [500, 'We could not create your password. Try again later.'],
  ] as const)(
    'shows the fallback for a %i response without backend detail',
    async (status, message) => {
      vi.mocked(postAuthSetupUser).mockResolvedValue({
        data: undefined,
        error: { status },
        response: new Response(null, { status }),
      })
      renderForm()

      await enterPassword('fourteen-chars')
      await submitForm()

      expect(await screen.findByText(message)).toBeInTheDocument()
      expect(mockPush).not.toHaveBeenCalled()
      expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
    },
  )

  it('preserves backend detail for a 500 response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: { detail: 'Backend explanation.', status: 500 },
      response: new Response(null, { status: 500 }),
    })
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(await screen.findByText('Backend explanation.')).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })

  it('shows a generic error if password setup fails without a response', async () => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({
      data: undefined,
      error: { status: 500 },
    })
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(
      await screen.findByText('We could not create your password. Try again later.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })

  it.each([
    { authenticationSession: 'test-authentication-session', otpAuthUri: '' },
    { authenticationSession: '', otpAuthUri: 'otpauth://totp/test' },
  ])('shows a generic error when successful response data is incomplete', async (data) => {
    vi.mocked(postAuthSetupUser).mockResolvedValue({ data, error: undefined })
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(
      await screen.findByText('We could not create your password. Try again later.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })

  it('shows a generic error if password setup fails unexpectedly', async () => {
    vi.mocked(postAuthSetupUser).mockRejectedValue(new Error('Network error'))
    renderForm()

    await enterPassword('fourteen-chars')
    await submitForm()

    expect(
      await screen.findByText('We could not create your password. Try again later.'),
    ).toBeInTheDocument()
    expect(mockPush).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(signUpMfaSetupStorageKey)).toBeNull()
  })
})
