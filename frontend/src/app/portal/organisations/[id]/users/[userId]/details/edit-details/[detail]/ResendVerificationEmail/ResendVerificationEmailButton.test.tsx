import '@testing-library/jest-dom/vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { resendWorkEmailVerification } from './actions'
import ResendVerificationEmailButton from './ResendVerificationEmailButton'

// Server actions can't run in jsdom, so replace the module with a mock.
vi.mock('./actions', () => ({
  resendWorkEmailVerification: vi.fn(),
}))

const mockAction = vi.mocked(resendWorkEmailVerification)

const navigation = { onSuccess: '/success', onError: '/error' }

const getButton = () => screen.getByRole('button', { name: /resend email/i })

describe('ResendVerificationEmailButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockAction.mockResolvedValue({ status: 'idle' })
  })

  it('renders an enabled submit button and no feedback initially', () => {
    render(<ResendVerificationEmailButton userId={1} navigation={navigation} />)

    const button = getButton()
    expect(button).toBeInTheDocument()
    expect(button).toHaveAttribute('type', 'submit')
    expect(button).toBeEnabled()

    expect(screen.queryByRole('status')).not.toBeInTheDocument()
    expect(screen.queryByText(/verification email sent/i)).not.toBeInTheDocument()
  })

  it('calls the action with the bound navigation and userId, the initial state and form data', async () => {
    const user = userEvent.setup()
    render(<ResendVerificationEmailButton userId={42} navigation={navigation} />)

    await user.click(getButton())

    await waitFor(() => expect(mockAction).toHaveBeenCalledTimes(1))
    expect(mockAction).toHaveBeenCalledWith(
      navigation,
      42,
      { status: 'idle' },
      expect.any(FormData),
    )
  })

  it('uses the userId passed in props', async () => {
    const user = userEvent.setup()
    const { rerender } = render(
      <ResendVerificationEmailButton userId={1} navigation={navigation} />,
    )

    await user.click(getButton())
    await waitFor(() =>
      expect(mockAction).toHaveBeenLastCalledWith(
        navigation,
        1,
        expect.anything(),
        expect.anything(),
      ),
    )

    rerender(<ResendVerificationEmailButton userId={2} navigation={navigation} />)
    await user.click(getButton())
    await waitFor(() =>
      expect(mockAction).toHaveBeenLastCalledWith(
        navigation,
        2,
        expect.anything(),
        expect.anything(),
      ),
    )
  })
})
