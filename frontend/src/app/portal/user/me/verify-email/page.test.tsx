import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import Page from './page'

// ---- Mocks -----------------------------------------------------------------

const {
  notFoundMock,
  redirectMock,
  getUsersMeMock,
  verifyPendingEmailAddressMock,
  createServerApiClientMock,
  buildSignInHrefMock,
} = vi.hoisted(() => ({
  notFoundMock: vi.fn(),
  redirectMock: vi.fn(),
  getUsersMeMock: vi.fn(),
  verifyPendingEmailAddressMock: vi.fn(),
  createServerApiClientMock: vi.fn(),
  buildSignInHrefMock: vi.fn(),
}))

vi.mock('server-only', () => ({}))

vi.mock('next/navigation', () => ({
  notFound: notFoundMock.mockImplementation(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
  redirect: redirectMock.mockImplementation((url: string) => {
    throw new Error(`NEXT_REDIRECT:${url}`)
  }),
}))

vi.mock('@/client/generated', () => ({
  getUsersMe: getUsersMeMock,
  verifyPendingEmailAddress: verifyPendingEmailAddressMock,
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: createServerApiClientMock,
}))

vi.mock('@/lib/auth/routing', () => ({
  buildSignInHref: buildSignInHrefMock,
}))

// ---- Tests -----------------------------------------------------------------

describe('Email verification page', () => {
  const client = { id: 'test-client' }
  const user = { userId: 'user-123' }
  const verificationToken = 'verification-token'

  beforeEach(() => {
    vi.clearAllMocks()

    notFoundMock.mockImplementation(() => {
      throw new Error('NEXT_NOT_FOUND')
    })

    redirectMock.mockImplementation((url: string) => {
      throw new Error(`NEXT_REDIRECT:${url}`)
    })

    createServerApiClientMock.mockResolvedValue(client)
    getUsersMeMock.mockResolvedValue({ data: user })
    verifyPendingEmailAddressMock.mockResolvedValue({ error: undefined })

    buildSignInHrefMock.mockReturnValue('/sign-in?emailVerified=true')
  })

  it('should return not found when no verification token is provided', async () => {
    await expect(Page({ searchParams: Promise.resolve({}) })).rejects.toThrow('NEXT_NOT_FOUND')

    expect(notFoundMock).toHaveBeenCalledOnce()
    expect(createServerApiClientMock).not.toHaveBeenCalled()
  })

  it('should return not found when the current user cannot be retrieved', async () => {
    getUsersMeMock.mockResolvedValue({ data: undefined })

    await expect(Page({ searchParams: Promise.resolve({ verificationToken }) })).rejects.toThrow(
      'NEXT_NOT_FOUND',
    )

    expect(notFoundMock).toHaveBeenCalledOnce()
    expect(verifyPendingEmailAddressMock).not.toHaveBeenCalled()
  })

  it('should render an error when email verification fails', async () => {
    verifyPendingEmailAddressMock.mockResolvedValue({
      error: {
        title: 'Verification failed',
        detail: 'The verification link is invalid or has expired.',
      },
    })

    const result = await Page({
      searchParams: Promise.resolve({ verificationToken }),
    })

    render(result)

    expect(screen.getByRole('heading', { name: 'Verification failed' })).toBeInTheDocument()

    expect(screen.getByText('The verification link is invalid or has expired.')).toBeInTheDocument()

    expect(redirectMock).not.toHaveBeenCalled()
  })

  it('should redirect to sign-in when email verification succeeds', async () => {
    await expect(Page({ searchParams: Promise.resolve({ verificationToken }) })).rejects.toThrow(
      'NEXT_REDIRECT:/sign-in?emailVerified=true',
    )

    expect(verifyPendingEmailAddressMock).toHaveBeenCalledWith({
      client,
      path: { userId: user.userId },
      body: { verificationToken },
    })

    expect(buildSignInHrefMock).toHaveBeenCalledWith(undefined, {
      emailVerified: true,
    })

    expect(redirectMock).toHaveBeenCalledWith('/sign-in?emailVerified=true')
  })
})
