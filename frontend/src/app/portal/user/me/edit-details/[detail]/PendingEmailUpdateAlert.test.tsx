import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import PendingEmailUpdateAlert from './PendingEmailUpdateAlert'

const email = 'new.address@example.com'

describe('PendingEmailUpdateAlert', () => {
  it('renders the heading and the target email address', () => {
    render(<PendingEmailUpdateAlert value={email} verifiedByEmail={false} />)

    expect(
      screen.getByRole('heading', { name: 'Email Update Pending', level: 2 }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('target-email')).toHaveTextContent(email)
  })

  describe('when the address has not been verified', () => {
    it('asks the user to verify the address', () => {
      render(<PendingEmailUpdateAlert value={email} verifiedByEmail={false} />)

      expect(screen.getByText(/sent a verification email to/i)).toBeInTheDocument()
      expect(screen.getByTestId('target-email-not-verified')).toHaveTextContent(email)
      expect(screen.queryByText(/this address has been verified/i)).not.toBeInTheDocument()
    })
  })

  describe('when the address has been verified', () => {
    it('tells the user to log in with the new address', () => {
      render(<PendingEmailUpdateAlert value={email} verifiedByEmail />)

      expect(screen.getByText(/this address has been verified/i)).toBeInTheDocument()
      expect(screen.queryByText(/sent a verification email to/i)).not.toBeInTheDocument()
      expect(screen.queryByTestId('target-email-not-verified')).not.toBeInTheDocument()
    })
  })
})
