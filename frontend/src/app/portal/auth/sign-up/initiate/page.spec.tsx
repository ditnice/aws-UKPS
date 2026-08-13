import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getAuthValidateSetupToken } from '@/client/generated/sdk.gen'

import SignUpInitiate from './page'

const redirect = vi.fn()

vi.mock('next/navigation', () => ({
  redirect: (url: string) => {
    redirect(url)
    throw new Error('NEXT_REDIRECT')
  },
}))

vi.mock('@/client/generated/sdk.gen', () => ({
  getAuthValidateSetupToken: vi.fn(),
}))

beforeEach(() => {
  vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
    data: undefined,
    error: undefined,
  })
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('SignUpInitiate', () => {
  it('renders an error if the setup token is missing', async () => {
    render(await SignUpInitiate({ searchParams: Promise.resolve({}) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeDefined()
    expect(screen.getByText('This sign-up link is missing a setup token.')).toBeDefined()
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
      '/portal/auth/sign-up/terms-and-conditions?setupToken=test-token',
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
      '/portal/auth/sign-up/terms-and-conditions?setupToken=test-token',
    )
  })

  it('renders backend content for an expired or consumed setup token', async () => {
    vi.mocked(getAuthValidateSetupToken).mockResolvedValue({
      data: undefined,
      error: {
        detail: 'The setup token has expired and can no longer be used.',
        status: 401,
        title: 'Setup token has expired.',
      },
      response: new Response(null, { status: 401 }),
    })

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('Setup token has expired.')).toBeDefined()
    expect(screen.getByText('The setup token has expired and can no longer be used.')).toBeDefined()
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

    expect(screen.getByText('Setup token not found.')).toBeDefined()
    expect(screen.getByText('The supplied setup token does not exist.')).toBeDefined()
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

    expect(screen.getByText('There is a problem with your sign-up link')).toBeDefined()
    expect(screen.getByText('This sign-up link is not valid.')).toBeDefined()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('renders a fallback error if validation fails unexpectedly', async () => {
    vi.mocked(getAuthValidateSetupToken).mockRejectedValue(new Error('Network error'))

    render(await SignUpInitiate({ searchParams: Promise.resolve({ setupToken: 'test-token' }) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeDefined()
    expect(screen.getByText('We could not check your sign-up link. Try again later.')).toBeDefined()
    expect(redirect).not.toHaveBeenCalled()
  })
})
