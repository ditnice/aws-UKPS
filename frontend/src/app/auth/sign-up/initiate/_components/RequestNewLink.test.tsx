import { act, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterAll, afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { postAuthResendSetupToken } from '@/client/generated'

import { RequestNewLink } from './RequestNewLink'

const supportEmail = vi.hoisted(() => {
  // The component reads this at module load, so it must be set before the import above runs
  const email = 'support@example.com'
  vi.stubEnv('NEXT_PUBLIC_QA_SUPPORT_EMAIL', email)
  return email
})

vi.mock('@/client/generated', () => ({
  postAuthResendSetupToken: vi.fn(),
}))

beforeEach(() => {
  vi.useFakeTimers({ shouldAdvanceTime: true })
  vi.mocked(postAuthResendSetupToken).mockResolvedValue({
    data: { correlationId: 'correlation-id-1' },
    error: undefined,
  })
})

afterAll(() => {})

afterEach(() => {
  vi.useRealTimers()
})

function getSendButton() {
  return screen.getByRole('button', { name: 'Send a new link' }) as HTMLButtonElement
}

function getSupportEmailLink() {
  return screen.getByRole('link', { name: supportEmail }) as HTMLAnchorElement
}

async function clickSend() {
  await user.click(getSendButton())
  await screen.findByText('Check your email')
}

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
})

describe('RequestNewLink', () => {
  it('renders the expired-link message with an enabled send button', () => {
    render(<RequestNewLink setupToken="test-token" />)

    expect(screen.getByText('This link has expired')).toBeInTheDocument()
    expect(
      screen.getByText(
        'Request a new link to continue setting up your account. A new link will be sent to your registered email address.',
      ),
    ).toBeInTheDocument()
    expect(getSendButton()).toBeEnabled()
  })

  it('sends the setup token and shows the check-your-email message on click', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    await user.click(getSendButton())

    expect(postAuthResendSetupToken).toHaveBeenCalledWith({
      body: { setupToken: 'test-token' },
    })

    expect(await screen.findByText('Check your email')).toBeInTheDocument()
    expect(
      screen.getByText(/We've sent a new link to your email address\. It may take a few minutes/),
    ).toBeInTheDocument()
    expect(
      screen.getByText(/If you cannot find the email, check your spam or junk folder\./),
    ).toBeInTheDocument()
    expect(screen.getByText('60 seconds')).toBeInTheDocument()
  })

  it('sends the returned correlation id, instead of the stale setup token, on a subsequent click', async () => {
    vi.mocked(postAuthResendSetupToken)
      .mockResolvedValueOnce({ data: { correlationId: 'correlation-id-1' }, error: undefined })
      .mockResolvedValueOnce({ data: { correlationId: 'correlation-id-2' }, error: undefined })

    render(<RequestNewLink setupToken="test-token" />)

    await clickSend()
    expect(postAuthResendSetupToken).toHaveBeenNthCalledWith(1, {
      body: { setupToken: 'test-token' },
    })

    await act(async () => {
      vi.advanceTimersByTime(60 * 1000)
    })
    await user.click(getSendButton())

    expect(postAuthResendSetupToken).toHaveBeenNthCalledWith(2, {
      body: { correlationId: 'correlation-id-1' },
    })
  })

  it('counts down the wait time once a second', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    await clickSend()
    expect(screen.getByText('60 seconds')).toBeInTheDocument()

    await act(async () => {
      vi.advanceTimersByTime(1000)
    })
    expect(screen.getByText('59 seconds')).toBeInTheDocument()

    await act(async () => {
      vi.advanceTimersByTime(58 * 1000)
    })
    expect(screen.getByText('1 second')).toBeInTheDocument()
  })

  it('disables the send button immediately after sending', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    await clickSend()

    expect(getSendButton()).toBeDisabled()
  })

  it('re-enables the send button once the cooldown has elapsed', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    await clickSend()
    expect(getSendButton()).toBeDisabled()

    await act(async () => {
      vi.advanceTimersByTime(60 * 1000)
    })

    expect(getSendButton()).toBeEnabled()
  })

  it('shows a contact-support message with a working email link for an unexpected failure status', async () => {
    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 500 },
      response: new Response(null, { status: 500 }),
    })

    render(<RequestNewLink setupToken="test-token" />)

    await user.click(getSendButton())

    expect(await screen.findByText('Contact the support team')).toBeInTheDocument()
    expect(
      screen.getByText(/Please contact the UKPS support team for assistance/),
    ).toBeInTheDocument()
    expect(getSupportEmailLink().getAttribute('href')).toBe(`mailto:${supportEmail}`)
    expect(screen.queryByRole('button', { name: 'Send a new link' })).toBeNull()
  })

  it('shows a contact-support message for a 404 response', async () => {
    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 404 },
      response: new Response(null, { status: 404 }),
    })

    render(<RequestNewLink setupToken="test-token" />)

    await user.click(getSendButton())

    expect(await screen.findByText('Contact the support team')).toBeInTheDocument()
    expect(getSupportEmailLink().getAttribute('href')).toBe(`mailto:${supportEmail}`)
    expect(screen.queryByRole('button', { name: 'Send a new link' })).toBeNull()
  })

  it('shows a contact-support message for a 400 response', async () => {
    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 400 },
      response: new Response(null, { status: 400 }),
    })

    render(<RequestNewLink setupToken="test-token" />)

    await user.click(getSendButton())

    expect(await screen.findByText('Contact the support team')).toBeInTheDocument()
  })

  it('shows a contact-support message if the request throws, e.g. a network failure', async () => {
    vi.mocked(postAuthResendSetupToken).mockRejectedValue(new Error('Network error'))

    render(<RequestNewLink setupToken="test-token" />)

    await user.click(getSendButton())

    expect(await screen.findByText('Contact the support team')).toBeInTheDocument()
  })

  it('allows up to three successful attempts', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    for (let attempt = 1; attempt <= 3; attempt++) {
      await clickSend()
      await act(async () => {
        vi.advanceTimersByTime(60 * 1000)
      })
    }

    expect(postAuthResendSetupToken).toHaveBeenCalledTimes(3)
    expect(screen.getByText('Check your email')).toBeInTheDocument()
    expect(getSendButton()).toBeEnabled()
  })

  it('shows contact support text once the backend rejects a fourth attempt', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    for (let attempt = 1; attempt <= 3; attempt++) {
      await user.click(getSendButton())
      await act(async () => {
        vi.advanceTimersByTime(60 * 1000)
      })
    }

    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 403 },
      response: new Response(null, { status: 403 }),
    })

    await user.click(getSendButton())

    expect(await screen.findByText('Contact the support team')).toBeInTheDocument()
    expect(
      screen.getByText(/You have reached the maximum number of attempts to request a new link\./),
    ).toBeInTheDocument()
    expect(getSupportEmailLink().getAttribute('href')).toBe(`mailto:${supportEmail}`)
    expect(screen.queryByRole('button', { name: 'Send a new link' })).toBeNull()
  })

  it('shows contact support text if the backend reports the resend limit has been reached', async () => {
    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 403 },
      response: new Response(null, { status: 403 }),
    })

    render(<RequestNewLink setupToken="test-token" />)

    await user.click(getSendButton())

    expect(await screen.findByText('Contact the support team')).toBeInTheDocument()
    expect(getSupportEmailLink().getAttribute('href')).toBe(`mailto:${supportEmail}`)
    expect(screen.queryByRole('button', { name: 'Send a new link' })).toBeNull()
  })
})
