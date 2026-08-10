import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { SignInForm } from './SignInForm'

afterEach(cleanup)

describe('SignInForm', () => {
  it('renders the sign-in controls', () => {
    render(<SignInForm />)

    expect(screen.getByLabelText('Email address')).toBeDefined()
    expect(screen.getByLabelText('Password')).toBeDefined()
    expect(screen.getByText('Forgotten your password?')).toBeDefined()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeDefined()
  })

  it('shows required validation errors when submitted empty', async () => {
    render(<SignInForm />)

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter your email address')).toBeDefined()
    expect(await screen.findByText('Enter your password')).toBeDefined()
  })

  it('shows an email format validation error', async () => {
    render(<SignInForm />)

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'not-an-email-address' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'secure-password' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(
      await screen.findByText(
        'Enter an email address in the correct format, like name@example.com',
      ),
    ).toBeDefined()
  })

  it('does not show validation errors for valid values', async () => {
    render(<SignInForm />)

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'name@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'secure-password' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(screen.queryByText('Enter your email address')).toBeNull()
      expect(
        screen.queryByText('Enter an email address in the correct format, like name@example.com'),
      ).toBeNull()
      expect(screen.queryByText('Enter your password')).toBeNull()
    })
  })

  it('revalidates fields on blur after a failed submit', async () => {
    render(<SignInForm />)

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter your email address')).toBeDefined()
    expect(await screen.findByText('Enter your password')).toBeDefined()

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'name@example.com' },
    })
    fireEvent.blur(screen.getByLabelText('Email address'))

    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'secure-password' },
    })
    fireEvent.blur(screen.getByLabelText('Password'))

    await waitFor(() => {
      expect(screen.queryByText('Enter your email address')).toBeNull()
      expect(screen.queryByText('Enter your password')).toBeNull()
    })
  })
})
