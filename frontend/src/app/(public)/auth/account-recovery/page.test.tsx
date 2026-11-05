import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import AccountRecovery from './page'

const supportEmail = vi.hoisted(() => {
  const email = 'support@example.com'
  vi.stubEnv('NEXT_PUBLIC_QA_SUPPORT_EMAIL', email)
  return email
})

describe('AccountRecovery', () => {
  it('explains how to get help recovering an account', () => {
    render(<AccountRecovery />)

    expect(screen.getByText('Recover your UK PharmaScan account')).toBeInTheDocument()
    expect(
      screen.getByText(
        'If you cannot recover your account, contact the UK PharmaScan support team for assistance. For example, you do not know both the email address or password you use to sign in.',
      ),
    ).toBeInTheDocument()
  })

  it('provides a mailto link to the support team', () => {
    render(<AccountRecovery />)

    expect(screen.getByRole('link', { name: supportEmail })).toHaveAttribute(
      'href',
      `mailto:${supportEmail}`,
    )
  })

  it('links back to sign-in', () => {
    render(<AccountRecovery />)

    expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute('href', '/auth/sign-in')
  })
})
