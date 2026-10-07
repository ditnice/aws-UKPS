import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import { postAuthLogin } from '@/client/generated'
import { routeOnSuccessfulAuth } from '@/lib/auth/routing'
import { router } from '@/test-utils/nextNavigation'
import { fillInput } from '@/test-utils/userInteractions'

import { SignInForm } from './SignInForm'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  postAuthLogin: vi.fn(),
}))

vi.mocked(postAuthLogin).mockResolvedValue({
  error: undefined,
  data: undefined,
})
let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('SignInForm', () => {
  it('renders the sign-in controls', () => {
    render(<SignInForm />)

    expect(screen.getByLabelText('Email address')).toBeInTheDocument()
    expect(screen.getByLabelText('Password')).toBeInTheDocument()
    expect(screen.getByText('Forgotten your password?')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeInTheDocument()
  })

  it('shows required validation errors when submitted empty', async () => {
    render(<SignInForm />)

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter your email address')).toBeInTheDocument()
    expect(await screen.findByText('Enter your password')).toBeInTheDocument()
  })

  it('shows an email format validation error', async () => {
    render(<SignInForm />)

    await fillInput(user, screen.getByLabelText('Email address'), 'not-an-email-address')
    await fillInput(user, screen.getByLabelText('Password'), 'secure-password')
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(
      await screen.findByText(
        'Enter an email address in the correct format, like name@example.com',
      ),
    ).toBeInTheDocument()
  })

  it('does not show validation errors for valid values', async () => {
    render(<SignInForm />)

    await fillInput(user, screen.getByLabelText('Email address'), 'name@example.com')
    await fillInput(user, screen.getByLabelText('Password'), 'secure-password')
    await user.click(screen.getByRole('button', { name: 'Continue' }))

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

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText('Enter your email address')).toBeInTheDocument()
    expect(await screen.findByText('Enter your password')).toBeInTheDocument()

    await fillInput(user, screen.getByLabelText('Email address'), 'name@example.com')
    await user.tab()

    await fillInput(user, screen.getByLabelText('Password'), 'secure-password')
    await user.tab()

    await waitFor(() => {
      expect(screen.queryByText('Enter your email address')).toBeNull()
      expect(screen.queryByText('Enter your password')).toBeNull()
    })
  })

  it('submits valid credentials and redirects to the portal', async () => {
    render(<SignInForm />)

    await fillInput(user, screen.getByLabelText('Email address'), 'name@example.com')
    await fillInput(user, screen.getByLabelText('Password'), 'secure-password')

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(postAuthLogin).toHaveBeenCalledWith({
        body: {
          username: 'name@example.com',
          password: 'secure-password',
        },
        credentials: 'include',
      })

      expect(router.push).toHaveBeenCalledWith(routeOnSuccessfulAuth)
    })
  })

  it('redirects to the returnTo path after successful authentication', async () => {
    render(<SignInForm returnTo="/portal/organisations/1?tab=users" />)

    await fillInput(user, screen.getByLabelText('Email address'), 'name@example.com')
    await fillInput(user, screen.getByLabelText('Password'), 'secure-password')

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(router.push).toHaveBeenCalledWith('/portal/organisations/1?tab=users')
    })
  })

  it('submits values that were autofilled without firing an input event', async () => {
    render(<SignInForm />)

    const emailInput = screen.getByLabelText('Email address')
    const passwordInput = screen.getByLabelText('Password')

    // Browser autofill can set an input's DOM value directly, bypassing the
    // native input event React relies on to update controlled state. Setting
    // the value through the native setter (rather than fireEvent.change, which
    // also dispatches that event) reproduces that gap.
    const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
      window.HTMLInputElement.prototype,
      'value',
    )!.set!
    nativeInputValueSetter.call(emailInput, 'autofilled@example.com')
    nativeInputValueSetter.call(passwordInput, 'autofilled-password')

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(postAuthLogin).toHaveBeenCalledWith({
        body: {
          username: 'autofilled@example.com',
          password: 'autofilled-password',
        },
        credentials: 'include',
      })
    })

    expect(screen.queryByText('Enter your email address')).toBeNull()
    expect(screen.queryByText('Enter your password')).toBeNull()
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

    await fillInput(user, screen.getByLabelText('Email address'), 'name@example.com')
    await fillInput(user, screen.getByLabelText('Password'), 'incorrect-password')

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    const errors = await screen.findAllByText(
      'The email address or password you entered is incorrect',
    )

    expect(errors).toHaveLength(2)

    expect(postAuthLogin).toHaveBeenCalledWith({
      body: {
        username: 'name@example.com',
        password: 'incorrect-password',
      },
      credentials: 'include',
    })

    expect(router.push).not.toHaveBeenCalled()
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

    await fillInput(user, screen.getByLabelText('Email address'), 'name@example.com')
    await fillInput(user, screen.getByLabelText('Password'), 'secure-password')

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(postAuthLogin).toHaveBeenCalledWith({
        body: {
          username: 'name@example.com',
          password: 'secure-password',
        },
        credentials: 'include',
      })

      expect(router.push).toHaveBeenCalledWith(
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

    await fillInput(user, screen.getByLabelText('Email address'), 'name@example.com')
    await fillInput(user, screen.getByLabelText('Password'), 'secure-password')

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    await waitFor(() => {
      expect(router.push).toHaveBeenCalledWith(
        '/auth/sign-in/mfa?username=name%40example.com&session=test-authentication-session&returnTo=%2Fportal%2Forganisations%2F1%3Ftab%3Dusers',
      )
    })
  })
})
