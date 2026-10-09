import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest'

import { postAuthMfa } from '@/client/generated'
import { routeOnSuccessfulAuth } from '@/lib/auth/routing'
import { errorMessages } from '@/lib/form/errorMessages'
import { router } from '@/test-utils/nextNavigation'
import { fillInput } from '@/test-utils/userInteractions'

import { SignInMfaForm } from './SignInMfaForm'

const { push: mockPush } = router

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  postAuthMfa: vi.fn(),
}))

const exampleUserEmail = 'user@email.com'
const exampleSession = 'session'

afterEach(() => {
  vi.restoreAllMocks()
})

type FormValues = { securityCode: string }
const renderValidForm = () => {
  render(<SignInMfaForm username={exampleUserEmail} session={exampleSession} />)
}
const validFormValues: FormValues = { securityCode: '123456' }
const submitForm = async () => {
  await user.click(screen.getByRole('button', { name: 'Continue' }))
}
const updateForm = async (formValues: FormValues) => {
  await fillInput(user, screen.getByLabelText('Security code'), formValues.securityCode)
}

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  vi.mocked(postAuthMfa).mockResolvedValue({ error: undefined, data: undefined })
  user = userEvent.setup()
})

describe('SignInMfaForm', () => {
  it('renders the MFA controls', () => {
    renderValidForm()
    expect(screen.getByLabelText('Security code')).toBeInTheDocument()
    expect(
      screen.getByText(`Enter the 6-digit authentication code shown in the app.`),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Contact UKPS support' })).toBeInTheDocument()
  })
  it('redirects on successful authentication', async () => {
    renderValidForm()
    await updateForm(validFormValues)
    await submitForm()
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
    await updateForm(validFormValues)
    await submitForm()
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
    await updateForm(validFormValues)
    await submitForm()
    expect(await screen.findByText(errorMessages.incorrectMfaCode)).toBeInTheDocument()
  })
  it('shows a required validation error when submitted empty', async () => {
    renderValidForm()
    await updateForm({ ...validFormValues, securityCode: '' })
    await submitForm()
    expect(await screen.findByText('Enter your security code')).toBeInTheDocument()
  })

  it('shows a format validation error for an invalid code', async () => {
    renderValidForm()
    await updateForm({ ...validFormValues, securityCode: '12345' })
    await submitForm()
    expect(await screen.findByText('Enter a 6-digit security code')).toBeInTheDocument()
  })

  it('does not show validation errors for a valid code', async () => {
    renderValidForm()
    await updateForm(validFormValues)
    await submitForm()
    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })

  it('accepts grouped security codes and submits the normalised code', async () => {
    renderValidForm()

    await updateForm({ ...validFormValues, securityCode: '12 324-6' })
    await submitForm()

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

    await submitForm()

    expect(await screen.findByText('Enter your security code')).toBeInTheDocument()

    await updateForm({ ...validFormValues, securityCode: '123 456' })
    await user.tab()

    await waitFor(() => {
      expect(screen.queryByText('Enter your security code')).toBeNull()
      expect(screen.queryByText('Enter a 6-digit security code')).toBeNull()
    })
  })
})
