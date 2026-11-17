import { render, screen } from '@testing-library/react'
import { notFound, redirect } from 'next/navigation'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getUsersMe, verifyPendingEmailAddress } from '@/client/generated'
import { buildSignInHref } from '@/lib/auth/routing'

import Page from './page'

vi.mock('@/client/generated', () => ({
  getUsersMe: vi.fn(),
  verifyPendingEmailAddress: vi.fn(),
}))

vi.mock('@/lib/auth/routing', () => ({
  buildSignInHref: vi.fn(() => '/sign-in?emailVerified=true'),
}))

vi.mock('next/navigation', () => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
  redirect: vi.fn((url: string) => {
    throw new Error(`NEXT_REDIRECT:${url}`)
  }),
}))

vi.mock('@/components/PageHeader/PageHeader', () => ({
  PageHeader: ({ heading }: { heading: string }) => <h1>{heading}</h1>,
}))

vi.mock('@/components/Placeholder/ErrorState', () => ({
  ErrorState: ({ children }: { children: React.ReactNode }) => <p role="alert">{children}</p>,
}))

const renderPage = async (searchParams: { verificationToken?: string }) =>
  Page({ searchParams: Promise.resolve(searchParams) })

describe('verify pending email page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(getUsersMe).mockResolvedValue({
      data: { userId: 123 },
    } as Awaited<ReturnType<typeof getUsersMe>>)
  })

  it('calls notFound when the verification token is missing', async () => {
    await expect(renderPage({})).rejects.toThrow('NEXT_NOT_FOUND')

    expect(notFound).toHaveBeenCalledOnce()
    expect(getUsersMe).not.toHaveBeenCalled()
    expect(verifyPendingEmailAddress).not.toHaveBeenCalled()
  })

  it('calls notFound when there is no signed-in user', async () => {
    vi.mocked(getUsersMe).mockResolvedValue({
      data: undefined,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    await expect(renderPage({ verificationToken: 'abc' })).rejects.toThrow('NEXT_NOT_FOUND')

    expect(verifyPendingEmailAddress).not.toHaveBeenCalled()
  })

  it('verifies the token for the current user and redirects to sign in', async () => {
    vi.mocked(verifyPendingEmailAddress).mockResolvedValue({
      error: undefined,
    } as Awaited<ReturnType<typeof verifyPendingEmailAddress>>)

    await expect(renderPage({ verificationToken: 'abc' })).rejects.toThrow(
      'NEXT_REDIRECT:/sign-in?emailVerified=true',
    )

    expect(verifyPendingEmailAddress).toHaveBeenCalledWith({
      path: { userId: 1 },
      body: { verificationToken: 'abc' },
    })
    expect(buildSignInHref).toHaveBeenCalledWith(undefined, {
      emailVerified: true,
    })
    expect(redirect).toHaveBeenCalledWith('/sign-in?emailVerified=true')
  })

  it('renders the error heading and detail when verification fails', async () => {
    vi.mocked(verifyPendingEmailAddress).mockResolvedValue({
      error: { title: 'Token expired', detail: 'Request a new link.' },
    } as Awaited<ReturnType<typeof verifyPendingEmailAddress>>)

    render(await renderPage({ verificationToken: 'abc' }))

    expect(screen.getByRole('heading', { name: 'Token expired' })).toBeInTheDocument()
    expect(screen.getByRole('alert')).toHaveTextContent('Request a new link.')
    expect(redirect).not.toHaveBeenCalled()
  })
})
