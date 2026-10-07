import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import SignUpSetPassword from './page'

vi.mock('./_components/SignUpSetPasswordForm', () => ({
  SignUpSetPasswordForm: ({ setupToken }: { setupToken: string }) => (
    <div>Set password form for {setupToken}</div>
  ),
}))
describe('SignUpSetPassword', () => {
  it('renders an error if the setup token is missing', async () => {
    render(await SignUpSetPassword({ searchParams: Promise.resolve({}) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeInTheDocument()
    expect(screen.getByText('This sign-up link is missing a setup token.')).toBeInTheDocument()
  })

  it('passes the setup token to the form', async () => {
    render(
      await SignUpSetPassword({
        searchParams: Promise.resolve({ setupToken: 'test-setup-token' }),
      }),
    )

    expect(screen.getByText('Create a password')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      '/auth/sign-up/terms-and-conditions?setupToken=test-setup-token',
    )
    expect(screen.getByText('Set password form for test-setup-token')).toBeInTheDocument()
  })
})
