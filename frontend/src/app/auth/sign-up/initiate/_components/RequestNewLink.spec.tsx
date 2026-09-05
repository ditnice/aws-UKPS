import { act, cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { postAuthResendSetupToken } from '@/client/generated'

import { RequestNewLink } from './RequestNewLink'

vi.mock('@/client/generated', () => ({
  postAuthResendSetupToken: vi.fn(),
}))

beforeEach(() => {
  vi.useFakeTimers({ shouldAdvanceTime: true })
  vi.mocked(postAuthResendSetupToken).mockResolvedValue({
    data: undefined,
    error: undefined,
  })
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
  vi.useRealTimers()
})

function getSendButton() {
  return screen.getByRole('button', { name: 'Send a new link' }) as HTMLButtonElement
}

async function clickSend() {
  fireEvent.click(getSendButton())
  await screen.findByText('Check your email')
}

describe('RequestNewLink', () => {
  it('renders the expired-link message with an enabled send button', () => {
    render(<RequestNewLink setupToken="test-token" />)

    expect(screen.getByText('This link has expired')).toBeDefined()
    expect(
      screen.getByText(
        'Request a new link to continue setting up your account. A new link will be sent to your registered email address.',
      ),
    ).toBeDefined()
    expect(getSendButton().disabled).toBe(false)
  })

  it('sends the setup token and shows the check-your-email message on click', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    fireEvent.click(getSendButton())

    expect(postAuthResendSetupToken).toHaveBeenCalledWith({
      body: { setupToken: 'test-token' },
    })

    expect(await screen.findByText('Check your email')).toBeDefined()
    expect(
      screen.getByText(/We've sent a new link to your email address\. It may take a few minutes/),
    ).toBeDefined()
    expect(
      screen.getByText(/If you cannot find the email, check your spam or junk folder\./),
    ).toBeDefined()
    expect(screen.getByText('60 seconds')).toBeDefined()
  })

  it('counts down the wait time once a second', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    await clickSend()
    expect(screen.getByText('60 seconds')).toBeDefined()

    await act(async () => {
      vi.advanceTimersByTime(1000)
    })
    expect(screen.getByText('59 seconds')).toBeDefined()

    await act(async () => {
      vi.advanceTimersByTime(58 * 1000)
    })
    expect(screen.getByText('1 second')).toBeDefined()
  })

  it('disables the send button immediately after sending', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    await clickSend()

    expect(getSendButton().disabled).toBe(true)
  })

  it('re-enables the send button once the cooldown has elapsed', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    await clickSend()
    expect(getSendButton().disabled).toBe(true)

    await act(async () => {
      vi.advanceTimersByTime(60 * 1000)
    })

    expect(getSendButton().disabled).toBe(false)
  })

  it('still shows the check-your-email message if sending fails', async () => {
    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 500 },
      response: new Response(null, { status: 500 }),
    })

    render(<RequestNewLink setupToken="test-token" />)

    fireEvent.click(getSendButton())

    expect(await screen.findByText('Check your email')).toBeDefined()
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
    expect(screen.getByText('Check your email')).toBeDefined()
    expect(getSendButton().disabled).toBe(false)
  })

  it('shows contact support text once the backend rejects a fourth attempt', async () => {
    render(<RequestNewLink setupToken="test-token" />)

    for (let attempt = 1; attempt <= 3; attempt++) {
      fireEvent.click(getSendButton())
      await act(async () => {
        vi.advanceTimersByTime(60 * 1000)
      })
    }

    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 403 },
      response: new Response(null, { status: 403 }),
    })

    fireEvent.click(getSendButton())

    expect(await screen.findByText('UKPS support')).toBeDefined()
    expect(screen.queryByRole('button', { name: 'Send a new link' })).toBeNull()
  })

  it('shows contact support text if the backend reports the resend limit has been reached', async () => {
    vi.mocked(postAuthResendSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 403 },
      response: new Response(null, { status: 403 }),
    })

    render(<RequestNewLink setupToken="test-token" />)

    fireEvent.click(getSendButton())

    expect(await screen.findByText('UKPS support')).toBeDefined()
    expect(screen.queryByRole('button', { name: 'Send a new link' })).toBeNull()
  })
})
