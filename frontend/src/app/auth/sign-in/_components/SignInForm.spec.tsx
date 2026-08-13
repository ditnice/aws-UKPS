import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import { postAuthLogin } from '@/client/generated'

import { routeOnSuccessfulAuth } from '../../constants'

import { SignInForm } from './SignInForm'

const mockPush = vi.fn()

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

vi.mock('@/client/generated', () => ({
  postAuthLogin: vi.fn(),
}))

vi.mocked(postAuthLogin).mockResolvedValue({
  error: undefined,
  data: undefined,
})

afterEach(cleanup)

beforeEach(() => {
  vi.clearAllMocks()
})

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

  it('submits valid credentials and redirects to the portal', async () => {
    render(<SignInForm />)

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'name@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'secure-password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(postAuthLogin).toHaveBeenCalledWith({
        body: {
          username: 'name@example.com',
          password: 'secure-password',
        },
        credentials: 'include',
      })

      expect(mockPush).toHaveBeenCalledWith(routeOnSuccessfulAuth)
    })
  })

  it('redirects to the returnTo path after successful authentication', async () => {
    render(<SignInForm returnTo="/portal/organisations/1?tab=users" />)

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'name@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'secure-password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith('/portal/organisations/1?tab=users')
    })
  })

  it('sets and shows an error message if the response is a 401', async () => {
    vi.mocked(postAuthLogin).mockResolvedValue({
      error: {
        status: 401,
        challengeType: undefined,
      },
      data: undefined,
    })

    render(<SignInForm />)

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'name@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'incorrect-password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    const errors = await screen.findAllByText('Invalid email or password')

    expect(errors).toHaveLength(2)

    expect(postAuthLogin).toHaveBeenCalledWith({
      body: {
        username: 'name@example.com',
        password: 'incorrect-password',
      },
      credentials: 'include',
    })

    expect(mockPush).not.toHaveBeenCalled()
  })

  it('redirects to the MFA page if there is an MFA challenge', async () => {
    vi.mocked(postAuthLogin).mockResolvedValue({
      error: {
        challengeType: 'MultiFactorAuthenticationRequired',
        authenticationSession: 'test-authentication-session',
      },
      data: undefined,
    })

    render(<SignInForm />)

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'name@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'secure-password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(postAuthLogin).toHaveBeenCalledWith({
        body: {
          username: 'name@example.com',
          password: 'secure-password',
        },
        credentials: 'include',
      })

      expect(mockPush).toHaveBeenCalledWith(
        '/auth/sign-in/mfa?username=name%40example.com&session=test-authentication-session',
      )
    })
  })

  it('passes returnTo to the MFA page if there is an MFA challenge', async () => {
    vi.mocked(postAuthLogin).mockResolvedValue({
      error: {
        challengeType: 'MultiFactorAuthenticationRequired',
        authenticationSession: 'test-authentication-session',
      },
      data: undefined,
    })

    render(<SignInForm returnTo="/portal/organisations/1?tab=users" />)

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'name@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'secure-password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(
        '/auth/sign-in/mfa?username=name%40example.com&session=test-authentication-session&returnTo=%2Fportal%2Forganisations%2F1%3Ftab%3Dusers',
      )
    })
  })
})
