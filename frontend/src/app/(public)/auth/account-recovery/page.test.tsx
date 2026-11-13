import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import AccountRecovery from './page'

vi.mock('next/server', () => ({
  connection: vi.fn(),
}))

const supportEmail = 'support@example.com'

describe('AccountRecovery', () => {
  beforeEach(() => {
    vi.stubEnv('QA_SUPPORT_EMAIL', supportEmail)
  })

  it('explains how to get help recovering an account', async () => {
    render(await AccountRecovery())

    expect(screen.getByText('Recover your UK PharmaScan account')).toBeInTheDocument()
    expect(
      screen.getByText(
        'If you cannot recover your account, contact the UK PharmaScan support team for assistance. For example, you do not know both the email address or password you use to sign in.',
      ),
    ).toBeInTheDocument()
  })

  it('provides a mailto link to the support team', async () => {
    render(await AccountRecovery())

    expect(screen.getByRole('link', { name: supportEmail })).toHaveAttribute(
      'href',
      `mailto:${supportEmail}`,
    )
  })

  it('explains how to get help when the support email is not configured', async () => {
    vi.stubEnv('QA_SUPPORT_EMAIL', '')

    render(await AccountRecovery())

    expect(screen.queryByRole('link', { name: supportEmail })).not.toBeInTheDocument()
    expect(
      screen.getByText('Please contact the UK PharmaScan support team for assistance.'),
    ).toBeInTheDocument()
  })

  it('links back to sign-in', async () => {
    render(await AccountRecovery())

    expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute('href', '/auth/sign-in')
  })
})
