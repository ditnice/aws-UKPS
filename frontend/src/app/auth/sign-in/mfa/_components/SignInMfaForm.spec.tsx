import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { postAuthMfa } from '@/client/generated'

import { routeOnSuccessfulAuth } from '../../../constants'

import { SignInMfaForm } from './SignInMfaForm'

const mockPush = vi.fn()

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

vi.mock('@/client/generated', () => ({
  postAuthMfa: vi.fn(),
}))

vi.mocked(postAuthMfa).mockResolvedValue({
  error: undefined,
  data: undefined,
})

const exampleUserEmail = 'user@email.com'
const exampleSession = 'session'

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})

type FormValues = { securityCode: string }
const renderValidForm = () => {
  render(<SignInMfaForm username={exampleUserEmail} session={exampleSession} />)
}
const validFormValues: FormValues = { securityCode: '123456' }
const submitForm = () => {
  fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
}
const updateForm = (formValues: FormValues) => {
  fireEvent.change(screen.getByLabelText('Security code'), {
    target: { value: formValues.securityCode },
  })
}

describe('SignInMfaForm', () => {
  it('renders the MFA controls', () => {
    renderValidForm()
    expect(screen.getByLabelText('Security code')).toBeDefined()
    expect(
      screen.getByText(
        `Enter the 6-digit authentication code shown in the app for ${exampleUserEmail}.`,
      ),
    ).toBeDefined()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Contact UKPS support' })).toBeDefined()
  })
  it('redirects on successful authentication', async () => {
    renderValidForm()
    updateForm(validFormValues)
    submitForm()
    await waitFor(() => {
      expect(postAuthMfa).toHaveBeenCalledWith({
        body: {
          authenticationSession: exampleSession,
          code: '123456',
          username: exampleUserEmail,
        },
        credentials: 'include',
      })
      expect(mockPush).toHaveBeenCalledWith(routeOnSuccessfulAuth)
    })
  })
  it('redirects to the returnTo path on successful authentication', async () => {
    render(
      <SignInMfaForm
        username={exampleUserEmail}
        session={exampleSession}
        returnTo="/portal/organisations/1?tab=users"
      />,
    )
    updateForm(validFormValues)
    submitForm()
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith('/portal/organisations/1?tab=users')
    })
  })
  it('shows security code error on 401 response', async () => {
    vi.mocked(postAuthMfa).mockResolvedValue({
      error: { status: 401 },
      data: undefined,
    })
    renderValidForm()
    updateForm(validFormValues)
    submitForm()
    expect(await screen.findByText('Invalid security code.')).toBeDefined()
  })
  it('shows a required validation error when submitted empty', async () => {
    renderValidForm()
    updateForm({ ...validFormValues, securityCode: '' })
    submitForm()
    expect(await screen.findByText('Enter your security code')).toBeDefined()
  })

  it('shows a format validation error for an invalid code', async () => {
    renderValidForm()
    updateForm({ ...validFormValues, securityCode: '12345' })
    submitForm()
    expect(await screen.findByText('Enter a 6-digit security code')).toBeDefined()
  })

  it('does not show validation errors for a valid code', async () => {
    renderValidForm()
    updateForm(validFormValues)
    submitForm()
    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })

  it('accepts grouped security codes and submits the normalised code', async () => {
    renderValidForm()

    updateForm({ ...validFormValues, securityCode: '12 324-6' })
    submitForm()

    await waitFor(() => {
      expect(postAuthMfa).toHaveBeenCalledWith({
        body: {
          authenticationSession: exampleSession,
          code: '123246',
          username: exampleUserEmail,
        },
        credentials: 'include',
      })
    })
  })

  it('revalidates fields on blur after a failed submit', async () => {
    renderValidForm()

    submitForm()

    expect(await screen.findByText('Enter your security code')).toBeDefined()

    updateForm({ ...validFormValues, securityCode: '123 456' })
    fireEvent.blur(screen.getByLabelText('Security code'))

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })
})
