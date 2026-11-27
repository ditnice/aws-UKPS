import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import SignInSmsMfa from './page'

// Isolate page wiring; MFA interactions are tested with the real form separately.
vi.mock('../_components/SignInMfaForm', () => ({
  SignInMfaForm: ({ returnTo }: { returnTo?: string }) => (
    <div>MFA form returnTo: {returnTo ?? 'none'}</div>
  ),
}))

// Direct invocation checks the returned synchronous tree, not Next.js rendering or hydration.
describe('SignInSmsMfa (direct invocation)', () => {
  it('asks the user to check their phone', async () => {
    render(
      await SignInSmsMfa({
        searchParams: Promise.resolve({
          username: 'name@example.com',
          session: 'test-session',
        }),
      }),
    )

    expect(screen.getByRole('heading', { name: 'Check your phone' })).toBeInTheDocument()
    expect(
      screen.getByText('We have sent you a text message with a security code.'),
    ).toBeInTheDocument()
  })

  it('does not pass an unsafe returnTo URL to the MFA form', async () => {
    render(
      await SignInSmsMfa({
        searchParams: Promise.resolve({
          username: 'name@example.com',
          session: 'test-session',
          returnTo: 'https://example.com/portal',
        }),
      }),
    )

    expect(screen.getByText('MFA form returnTo: none')).toBeInTheDocument()
  })
})
