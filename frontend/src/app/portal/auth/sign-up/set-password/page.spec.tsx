import { render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import SignUpSetPassword from './page'

vi.mock('./_components/SignUpSetPasswordForm', () => ({
  SignUpSetPasswordForm: ({ setupToken }: { setupToken: string }) => (
    <div>Set password form for {setupToken}</div>
  ),
}))

afterEach(() => {
  vi.clearAllMocks()
})

describe('SignUpSetPassword', () => {
  it('renders an error if the setup token is missing', async () => {
    render(await SignUpSetPassword({ searchParams: Promise.resolve({}) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeDefined()
    expect(screen.getByText('This sign-up link is missing a setup token.')).toBeDefined()
  })

  it('passes the setup token to the form', async () => {
    render(
      await SignUpSetPassword({
        searchParams: Promise.resolve({ setupToken: 'test-setup-token' }),
      }),
    )

    expect(screen.getByText('Create a password')).toBeDefined()
    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      '/portal/auth/sign-up/terms-and-conditions?setupToken=test-setup-token',
    )
    expect(screen.getByText('Set password form for test-setup-token')).toBeDefined()
  })
})
