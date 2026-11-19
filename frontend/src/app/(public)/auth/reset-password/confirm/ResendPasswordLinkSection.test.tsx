import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import ResetPasswordForm from '../ResetPasswordForm'

import { useCountdown } from './countdown'
import ResendPasswordLinkSection from './ResendPasswordLinkSection'

vi.mock('../ResetPasswordForm', () => ({
  default: vi.fn(() => <div data-testid="reset-password-form" />),
}))

vi.mock('./countdown', () => ({
  useCountdown: vi.fn(),
}))

const mockRestart = vi.fn()

const mockUseCountdown = vi.mocked(useCountdown)
const mockResetPasswordForm = vi.mocked(ResetPasswordForm)

describe('ResendPasswordLinkSection', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    mockUseCountdown.mockReturnValue({
      seconds: 60,
      restart: mockRestart,
    })
  })

  it('displays the confirmation message containing the email address', () => {
    render(<ResendPasswordLinkSection email="test@example.com" />)

    expect(
      screen.getByText(
        'An email containing a confirmation code has been sent to test@example.com.',
      ),
    ).toBeInTheDocument()
  })

  it('displays the countdown and hides the resend form', () => {
    render(<ResendPasswordLinkSection email="test@example.com" />)

    expect(screen.getByText('in 60 seconds')).toBeInTheDocument()

    const formContainer = screen.getByTestId('reset-password-form').parentElement

    expect(formContainer).toHaveClass('invisible')
    expect(formContainer).toHaveAttribute('inert')
  })

  it('passes the email address and resend button content to the form', () => {
    render(<ResendPasswordLinkSection email="test@example.com" />)

    expect(mockResetPasswordForm).toHaveBeenCalledWith(
      expect.objectContaining({
        email: 'test@example.com',
        buttonContentOverride: 'Resend link',
        onSuccess: expect.any(Function),
      }),
      undefined,
    )
  })

  it('restarts the countdown and focuses the message after a successful resend', () => {
    render(<ResendPasswordLinkSection email="test@example.com" />)

    const message = screen.getByText(
      'An email containing a confirmation code has been sent to test@example.com.',
    ).parentElement!

    const { onSuccess } = mockResetPasswordForm.mock.calls[0][0]

    onSuccess?.()

    expect(mockRestart).toHaveBeenCalledOnce()
    expect(message).toHaveFocus()
  })

  it('makes the resend form available when the countdown expires', () => {
    mockUseCountdown.mockReturnValue({
      seconds: 0,
      restart: mockRestart,
    })

    render(<ResendPasswordLinkSection email="test@example.com" />)

    expect(screen.queryByText(/in \d+ seconds/)).not.toBeInTheDocument()

    const formContainer = screen.getByTestId('reset-password-form').parentElement

    expect(formContainer).not.toHaveClass('invisible')
    expect(formContainer).not.toHaveAttribute('inert')
  })

  it('announces that another link can be requested when the countdown expires', () => {
    mockUseCountdown.mockReturnValue({
      seconds: 0,
      restart: mockRestart,
    })

    render(<ResendPasswordLinkSection email="test@example.com" />)

    expect(screen.getByRole('status')).toHaveTextContent('You can now request another link.')
  })
})
