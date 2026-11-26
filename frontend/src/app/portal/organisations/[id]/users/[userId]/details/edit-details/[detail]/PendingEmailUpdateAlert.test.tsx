import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import PendingEmailUpdateAlert from './PendingEmailUpdateAlert'

vi.mock('./editUserEmail/CancelPendingEmailChange', () => ({
  default: ({ userId }: { userId: number }) => (
    <button data-testid="cancel-verification-email-button" data-user-id={userId}>
      Resend verification email
    </button>
  ),
}))

vi.mock('./ResendVerificationEmail/ResendVerificationEmailButton', () => ({
  default: ({ userId }: { userId: number }) => (
    <button data-testid="resend-verification-email-button" data-user-id={userId}>
      Resend verification email
    </button>
  ),
}))

const email = 'new.address@example.com'
const userId = 123

type Props = Parameters<typeof PendingEmailUpdateAlert>[0]

// Resolve the async component, then render the JSX it returns
async function renderAlert(props: Partial<Props>) {
  const defaultProps: Props = {
    value: email,
    userId,
    verifiedByEmail: false,
    detail: 'email',
    organisationId: 7,
    returnLink: '/portal/organisations/7/users/123/details',
    onSuccessFlag: 'updated',
    verificationLinkResent: undefined,
    error: undefined,
  }
  const ui = await PendingEmailUpdateAlert({ ...defaultProps, ...props } as Props)
  return render(ui)
}

describe('PendingEmailUpdateAlert', () => {
  it('renders the heading and the target email address', async () => {
    await renderAlert({ value: email, verifiedByEmail: false, userId })

    expect(
      screen.getByRole('heading', { name: 'Email update pending', level: 3 }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('target-email')).toHaveTextContent(email)
  })

  it('renders the resend verification email button with the user id when verifiedByEmail is false', async () => {
    await renderAlert({ value: email, verifiedByEmail: false, userId })

    expect(screen.getByTestId('resend-verification-email-button')).toHaveAttribute(
      'data-user-id',
      String(userId),
    )
  })

  it('renders the cancel verification button with the user id when verifiedByEmail is false', async () => {
    await renderAlert({ value: email, verifiedByEmail: false, userId })

    expect(screen.getByTestId('cancel-verification-email-button')).toHaveAttribute(
      'data-user-id',
      String(userId),
    )
  })

  it('does not render the resend verification email button with the user id when verifiedByEmail is true', async () => {
    await renderAlert({ value: email, verifiedByEmail: true, userId })

    expect(screen.queryByTestId('resend-verification-email-button')).not.toBeInTheDocument()
  })

  it('renders the cancel verification email button with the user id when verifiedByEmail is true', async () => {
    await renderAlert({ value: email, verifiedByEmail: true, userId })

    expect(screen.getByTestId('cancel-verification-email-button')).toHaveAttribute(
      'data-user-id',
      String(userId),
    )
  })

  describe('when the address has not been verified', () => {
    it('asks the user to verify the address', async () => {
      await renderAlert({ value: email, verifiedByEmail: false, userId })

      expect(screen.getByText(/sent a verification email to/i)).toBeInTheDocument()
      expect(screen.getByTestId('target-email-not-verified')).toHaveTextContent(email)
      expect(screen.queryByText(/this address has been verified/i)).not.toBeInTheDocument()
    })
  })

  describe('when the address has been verified', () => {
    it('tells the user to log in with the new address', async () => {
      await renderAlert({ value: email, verifiedByEmail: true, userId })

      expect(screen.getByText(/this address has been verified/i)).toBeInTheDocument()
      expect(screen.queryByText(/sent a verification email to/i)).not.toBeInTheDocument()
    })
  })
})
