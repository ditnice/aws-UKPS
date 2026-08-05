import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import SignUpSetMfa from './page'

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})

describe('SignUpSetMfa', () => {
  it('renders the MFA setup controls', () => {
    render(<SignUpSetMfa />)

    expect(screen.getByRole('heading', { name: 'Set up two-factor authentication' })).toBeDefined()
    expect(screen.getByLabelText('QR code for authenticator app setup')).toBeDefined()
    expect(screen.getByText('JBSWY3DPEHPK3PXP')).toBeDefined()
    expect(screen.getByLabelText('Security code')).toBeDefined()
    expect(
      screen.getByText('Enter the 6-digit authentication code shown in the app.'),
    ).toBeDefined()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeDefined()
  })

  it('sets autocomplete for a one-time code', () => {
    render(<SignUpSetMfa />)

    expect(screen.getByLabelText('Security code').getAttribute('autocomplete')).toBe(
      'one-time-code',
    )
  })

  it('shows a required validation error when submitted empty', async () => {
    render(<SignUpSetMfa />)

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter your security code')).toBeDefined()
  })

  it('shows a format validation error for an invalid code', async () => {
    render(<SignUpSetMfa />)

    fireEvent.change(screen.getByLabelText('Security code'), {
      target: { value: '12345' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter a 6-digit security code')).toBeDefined()
  })

  it('does not show validation errors for a valid code', async () => {
    vi.spyOn(console, 'log').mockImplementation(() => undefined)
    render(<SignUpSetMfa />)

    fireEvent.change(screen.getByLabelText('Security code'), {
      target: { value: '123456' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })

  it('accepts grouped security codes and logs the normalised code', async () => {
    const consoleLog = vi.spyOn(console, 'log').mockImplementation(() => undefined)
    render(<SignUpSetMfa />)

    fireEvent.change(screen.getByLabelText('Security code'), {
      target: { value: '12 324-6' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(consoleLog).toHaveBeenCalledWith('123246')
    })
  })

  it('revalidates fields on blur after a failed submit', async () => {
    render(<SignUpSetMfa />)

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter your security code')).toBeDefined()

    fireEvent.change(screen.getByLabelText('Security code'), {
      target: { value: '123 456' },
    })
    fireEvent.blur(screen.getByLabelText('Security code'))

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })
})
