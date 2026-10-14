import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import SignIn from './page'

// Isolate returnTo prop wiring; form interactions are tested with the real form separately.
vi.mock('./_components/SignInForm', () => ({
  SignInForm: ({ returnTo }: { returnTo?: string }) => (
    <div>Sign in form returnTo: {returnTo ?? 'none'}</div>
  ),
}))
// Direct invocation checks the returned synchronous tree, not Next.js rendering or hydration.
describe('SignIn (direct invocation)', () => {
  it('passes a safe returnTo path to the sign-in form', async () => {
    render(
      await SignIn({
        searchParams: Promise.resolve({ returnTo: '/portal/organisations/1?tab=users' }),
      }),
    )

    expect(
      screen.getByText('Sign in form returnTo: /portal/organisations/1?tab=users'),
    ).toBeInTheDocument()
  })

  it('does not pass an unsafe returnTo URL to the sign-in form', async () => {
    render(
      await SignIn({
        searchParams: Promise.resolve({ returnTo: 'https://example.com/portal' }),
      }),
    )

    expect(screen.getByText('Sign in form returnTo: none')).toBeInTheDocument()
  })

  it('does not pass a protocol-relative returnTo URL to the sign-in form', async () => {
    render(
      await SignIn({
        searchParams: Promise.resolve({ returnTo: '//example.com/portal' }),
      }),
    )

    expect(screen.getByText('Sign in form returnTo: none')).toBeInTheDocument()
  })
})
