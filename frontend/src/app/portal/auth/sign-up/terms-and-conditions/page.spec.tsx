import { render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import SignUpTermsAndConditions from './page'

afterEach(() => {
  vi.clearAllMocks()
})

describe('SignUpTermsAndConditions', () => {
  it('renders an error if the setup token is missing', async () => {
    render(await SignUpTermsAndConditions({ searchParams: Promise.resolve({}) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeDefined()
    expect(screen.getByText('This sign-up link is missing a setup token.')).toBeDefined()
  })

  it('renders terms and links to set-password with the setup token', async () => {
    render(
      await SignUpTermsAndConditions({
        searchParams: Promise.resolve({ setupToken: 'test-setup-token' }),
      }),
    )

    expect(screen.getByRole('heading', { name: 'Terms and conditions' })).toBeDefined()
    expect(
      screen.getByText('Read and accept the terms and conditions before continuing.'),
    ).toBeDefined()
    expect(screen.getByRole('link', { name: 'Accept and continue' }).getAttribute('href')).toBe(
      '/portal/auth/sign-up/set-password?setupToken=test-setup-token',
    )
  })
})
