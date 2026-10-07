import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import SignUpTermsAndConditions from './page'

describe('SignUpTermsAndConditions', () => {
  it('renders an error if the setup token is missing', async () => {
    render(await SignUpTermsAndConditions({ searchParams: Promise.resolve({}) }))

    expect(screen.getByText('There is a problem with your sign-up link')).toBeInTheDocument()
    expect(screen.getByText('This sign-up link is missing a setup token.')).toBeInTheDocument()
  })

  it('renders terms and links to set-password with the setup token', async () => {
    render(
      await SignUpTermsAndConditions({
        searchParams: Promise.resolve({ setupToken: 'test-setup-token' }),
      }),
    )

    expect(screen.getByRole('heading', { name: 'Terms and conditions' })).toBeInTheDocument()
    expect(
      screen.getByText('Read and accept the terms and conditions before continuing.'),
    ).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Accept and continue' }).getAttribute('href')).toBe(
      '/auth/sign-up/set-password?setupToken=test-setup-token',
    )
  })
})
