import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { SignUpSetPasswordForm } from './SignUpSetPasswordForm'

afterEach(cleanup)

describe('SignUpSetPasswordForm', () => {
  it('renders the set password controls', () => {
    render(<SignUpSetPasswordForm />)

    expect(screen.getByText('Your password must:')).toBeDefined()
    expect(screen.getByText('be at least 8 characters long')).toBeDefined()
    expect(screen.getByLabelText('Password')).toBeDefined()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeDefined()
  })

  it('sets autocomplete for a new password', () => {
    render(<SignUpSetPasswordForm />)

    expect(screen.getByLabelText('Password').getAttribute('autocomplete')).toBe('new-password')
  })

  it('shows a required validation error when submitted empty', async () => {
    render(<SignUpSetPasswordForm />)

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter your password')).toBeDefined()
  })

  it('shows a minimum length validation error for a short password', async () => {
    render(<SignUpSetPasswordForm />)

    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'short' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Password must be at least 8 characters long')).toBeDefined()
  })

  it('does not show validation errors for a valid password', async () => {
    render(<SignUpSetPasswordForm />)

    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'fourteen-chars' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(screen.queryByText('Enter your password')).toBeNull()
      expect(screen.queryByText('Password must be at least 8 characters long')).toBeNull()
    })
  })

  it('revalidates the password on blur after a failed submit', async () => {
    render(<SignUpSetPasswordForm />)

    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'short' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Password must be at least 8 characters long')).toBeDefined()

    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'fourteen-chars' },
    })
    fireEvent.blur(screen.getByLabelText('Password'))

    await waitFor(() => {
      expect(screen.queryByText('Password must be at least 8 characters long')).toBeNull()
    })
  })
})
