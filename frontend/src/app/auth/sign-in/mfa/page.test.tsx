import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import SignInMfa from './page'

vi.mock('./_components/SignInMfaForm', () => ({
  SignInMfaForm: ({ returnTo }: { returnTo?: string }) => (
    <div>MFA form returnTo: {returnTo ?? 'none'}</div>
  ),
}))
describe('SignInMfa', () => {
  it('passes a safe returnTo path to the MFA form', async () => {
    render(
      await SignInMfa({
        searchParams: Promise.resolve({
          username: 'name@example.com',
          session: 'test-session',
          returnTo: '/portal/organisations/1?tab=users',
        }),
      }),
    )

    expect(screen.getByText('MFA form returnTo: /portal/organisations/1?tab=users')).toBeDefined()
  })

  it('does not pass an unsafe returnTo URL to the MFA form', async () => {
    render(
      await SignInMfa({
        searchParams: Promise.resolve({
          username: 'name@example.com',
          session: 'test-session',
          returnTo: 'https://example.com/portal',
        }),
      }),
    )

    expect(screen.getByText('MFA form returnTo: none')).toBeDefined()
  })
})
