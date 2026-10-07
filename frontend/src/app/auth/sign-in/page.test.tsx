import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import SignIn from './page'

vi.mock('./_components/SignInForm', () => ({
  SignInForm: ({ returnTo }: { returnTo?: string }) => (
    <div>Sign in form returnTo: {returnTo ?? 'none'}</div>
  ),
}))

afterEach(cleanup)

describe('SignIn', () => {
  it('passes a safe returnTo path to the sign-in form', async () => {
    render(
      await SignIn({
        searchParams: Promise.resolve({ returnTo: '/portal/organisations/1?tab=users' }),
      }),
    )

    expect(
      screen.getByText('Sign in form returnTo: /portal/organisations/1?tab=users'),
    ).toBeDefined()
  })

  it('does not pass an unsafe returnTo URL to the sign-in form', async () => {
    render(
      await SignIn({
        searchParams: Promise.resolve({ returnTo: 'https://example.com/portal' }),
      }),
    )

    expect(screen.getByText('Sign in form returnTo: none')).toBeDefined()
  })

  it('does not pass a protocol-relative returnTo URL to the sign-in form', async () => {
    render(
      await SignIn({
        searchParams: Promise.resolve({ returnTo: '//example.com/portal' }),
      }),
    )

    expect(screen.getByText('Sign in form returnTo: none')).toBeDefined()
  })
})
