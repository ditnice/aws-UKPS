import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getAuthValidateSetupToken } from '@/client/generated/sdk.gen'
import { redirect } from '@/test-utils/nextNavigation'

import SignUpInitiate from './page'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated/sdk.gen', () => ({
  getAuthValidateSetupToken: vi.fn(),
}))

beforeEach(() => {
  vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
    data: undefined,
    error: undefined,
  })
})
// Direct invocation checks orchestration and the returned synchronous tree only.
// It does not exercise Next.js rendering, routing, hydration or redirect handling.
describe('SignUpInitiate (direct invocation)', () => {
  it('renders an error if the setup token is missing', async () => {
    render(await SignUpInitiate({ searchParams: Promise.resolve({}) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeInTheDocument()
    expect(screen.getByText('This sign-up link is missing a setup token.')).toBeInTheDocument()
    expect(getAuthValidateSetupToken).not.toHaveBeenCalled()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('validates the setup token and redirects to terms and conditions', async () => {
    await expect(
      SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }),
    ).rejects.toThrow('NEXT_REDIRECT')

    expect(getAuthValidateSetupToken).toHaveBeenCalledWith({
      query: { setupToken: 'test-token' },
    })
    expect(redirect).toHaveBeenCalledWith(
      '/auth/sign-up/terms-and-conditions?setupToken=test-token',
    )
  })

  it('trims the setup token before validation and redirecting', async () => {
    await expect(
      SignUpInitiate({ searchParams: Promise.resolve({ setupToken: ' test-token ' }) }),
    ).rejects.toThrow('NEXT_REDIRECT')

    expect(getAuthValidateSetupToken).toHaveBeenCalledWith({
      query: { setupToken: 'test-token' },
    })
    expect(redirect).toHaveBeenCalledWith(
      '/auth/sign-up/terms-and-conditions?setupToken=test-token',
    )
  })

  it('renders a dedicated expired-link message and a resend action for an expired setup token', async () => {
    vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The setup token has expired and can no longer be used.',
        status: 410,
        title: 'Setup token has expired.',
      },
      response: new Response(null, { status: 410 }),
    })

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('This link has expired')).toBeInTheDocument()
    expect(
      screen.getByText(
        'Request a new link to continue setting up your account. A new link will be sent to your registered email address.',
      ),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Send a new link' })).toBeInTheDocument()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('renders backend content for a consumed setup token', async () => {
    vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The setup token has already been consumed and cannot be used again.',
        status: 409,
        title: 'Setup token has already been used.',
      },
      response: new Response(null, { status: 409 }),
    })

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('Setup token has already been used.')).toBeInTheDocument()
    expect(
      screen.getByText('The setup token has already been consumed and cannot be used again.'),
    ).toBeInTheDocument()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('renders backend content for a setup token that cannot be found', async () => {
    vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The supplied setup token does not exist.',
        status: 404,
        title: 'Setup token not found.',
      },
      response: new Response(null, { status: 404 }),
    })

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('Setup token not found.')).toBeInTheDocument()
    expect(screen.getByText('The supplied setup token does not exist.')).toBeInTheDocument()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('renders a fallback error for an invalid setup token request', async () => {
    vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
      data: undefined,
      error: {
        status: 400,
      },
      response: new Response(null, { status: 400 }),
    })

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeInTheDocument()
    expect(screen.getByText('This sign-up link is not valid.')).toBeInTheDocument()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('renders a generic fallback if validation fails without a response', async () => {
    vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
      data: undefined,
      error: { status: 500 },
    })

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeInTheDocument()
    expect(
      screen.getByText('We could not check your sign-up link. Try again later.'),
    ).toBeInTheDocument()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('renders a fallback error if validation fails unexpectedly', async () => {
    vi.mocked(getAuthValidateSetupToken).mockRejectedValue(new Error('Network error'))

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeInTheDocument()
    expect(
      screen.getByText('We could not check your sign-up link. Try again later.'),
    ).toBeInTheDocument()
    expect(redirect).not.toHaveBeenCalled()
  })
})
