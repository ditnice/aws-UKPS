import '@testing-library/jest-dom/vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { resendWorkEmailVerification } from './actions'
import ResendVerificationEmailButton from './ResendVerificationEmailButton'

// Server actions can't run in jsdom, so replace the module with a mock.
vi.mock('./actions', () => ({
  resendWorkEmailVerification: vi.fn(),
}))

const mockAction = vi.mocked(resendWorkEmailVerification)

const getButton = () => screen.getByRole('button', { name: /resend email/i })

describe('ResendVerificationEmailButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockAction.mockResolvedValue({ status: 'idle' })
  })

  it('renders an enabled submit button and no feedback initially', () => {
    render(<ResendVerificationEmailButton userId={1} />)

    const button = getButton()
    expect(button).toBeInTheDocument()
    expect(button).toHaveAttribute('type', 'submit')
    expect(button).toBeEnabled()

    expect(screen.queryByRole('status')).not.toBeInTheDocument()
    expect(screen.queryByText(/verification email sent/i)).not.toBeInTheDocument()
  })

  it('calls the action with the bound userId, the initial state and form data', async () => {
    const user = userEvent.setup()
    render(<ResendVerificationEmailButton userId={42} />)

    await user.click(getButton())

    await waitFor(() => expect(mockAction).toHaveBeenCalledTimes(1))
    expect(mockAction).toHaveBeenCalledWith(42, { status: 'idle' }, expect.any(FormData))
  })

  it('uses the userId passed in props', async () => {
    const user = userEvent.setup()
    const { rerender } = render(<ResendVerificationEmailButton userId={1} />)

    await user.click(getButton())
    await waitFor(() =>
      expect(mockAction).toHaveBeenLastCalledWith(1, expect.anything(), expect.anything()),
    )

    rerender(<ResendVerificationEmailButton userId={2} />)
    await user.click(getButton())
    await waitFor(() =>
      expect(mockAction).toHaveBeenLastCalledWith(2, expect.anything(), expect.anything()),
    )
  })

  it('shows a success message when the action succeeds', async () => {
    mockAction.mockResolvedValue({ status: 'success' })
    const user = userEvent.setup()
    render(<ResendVerificationEmailButton userId={1} />)

    await user.click(getButton())

    const status = await screen.findByRole('status')
    expect(status).toHaveTextContent('Verification email sent. Check your inbox.')
  })

  it('shows the error message when the action fails', async () => {
    mockAction.mockResolvedValue({ status: 'error', message: 'Too many requests' })
    const user = userEvent.setup()
    render(<ResendVerificationEmailButton userId={1} />)

    await user.click(getButton())

    expect(await screen.findByText('Too many requests')).toBeInTheDocument()
    expect(screen.queryByRole('status')).not.toBeInTheDocument()
  })

  it('disables the button while the action is pending and re-enables it afterwards', async () => {
    let resolveAction!: (value: { status: 'success' }) => void
    mockAction.mockReturnValue(
      new Promise((resolve) => {
        resolveAction = resolve
      }),
    )

    const user = userEvent.setup()
    render(<ResendVerificationEmailButton userId={1} />)

    await user.click(getButton())

    await waitFor(() => expect(getButton()).toBeDisabled())

    await act(async () => {
      resolveAction({ status: 'success' })
    })

    await waitFor(() => expect(getButton()).toBeEnabled())
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('replaces an earlier error with a success message on retry', async () => {
    mockAction
      .mockResolvedValueOnce({ status: 'error', message: 'Something went wrong' })
      .mockResolvedValueOnce({ status: 'success' })

    const user = userEvent.setup()
    render(<ResendVerificationEmailButton userId={1} />)

    await user.click(getButton())
    expect(await screen.findByText('Something went wrong')).toBeInTheDocument()

    await user.click(getButton())
    expect(await screen.findByRole('status')).toHaveTextContent(/verification email sent/i)
    expect(screen.queryByText('Something went wrong')).not.toBeInTheDocument()
  })
})
