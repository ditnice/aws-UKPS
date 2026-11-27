import { initialFormState } from '@tanstack/react-form-nextjs'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import FormLevelErrors from '@/lib/form/FormLevelErrors'

import { resetPasswordAction } from './resetPasswordAction'
import ResetPasswordForm from './ResetPasswordForm'

vi.mock('./resetPasswordAction', () => ({
  resetPasswordAction: vi.fn(),
}))

vi.mock('@/lib/form/FormLevelErrors', () => ({
  default: vi.fn(() => null),
}))

const mockAction = vi.mocked(resetPasswordAction)

type FormState = typeof initialFormState
type ResetPasswordFormProps = Parameters<typeof ResetPasswordForm>[0]
type ActionArgs = [redirectOnSuccess: boolean, prev: unknown, formData: FormData]

/** Builds the state the server action would return, merged over the initial state. */
const serverState = (overrides: Partial<FormState> = {}): FormState =>
  ({ ...initialFormState, ...overrides }) as FormState

/** The state the server action returns when the link was sent without redirecting. */
const successState = () => ({ success: true, ...initialFormState }) as FormState

const actionCall = (index: number) => mockAction.mock.calls[index] as unknown as ActionArgs

/** Renders the form and returns the elements/helpers every test needs. */
const setup = (actionResult: FormState = serverState(), props: ResetPasswordFormProps = {}) => {
  // Return a fresh object per call, as the real server action does.
  mockAction.mockImplementation(async () => ({ ...actionResult }) as never)

  const user = userEvent.setup()
  render(<ResetPasswordForm {...props} />)

  const emailInput = () => screen.getByLabelText('Email address')
  const submitButton = () =>
    screen.getByRole('button', { name: props.buttonContentOverride ?? 'Send link' })

  const submitWithEmail = async (email: string) => {
    await user.type(emailInput(), email)
    await user.click(submitButton())
  }

  return { user, emailInput, submitButton, submitWithEmail }
}

describe('ResetPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('initial render', () => {
    it('renders an empty email input with the correct attributes', () => {
      const { emailInput } = setup()

      expect(emailInput()).toHaveValue('')
      expect(emailInput()).toHaveAttribute('type', 'email')
      expect(emailInput()).toHaveAttribute('name', 'email')
      expect(emailInput()).toHaveAttribute('autocomplete', 'email')
    })

    it('renders an enabled submit button', () => {
      const { submitButton } = setup()

      expect(submitButton()).toBeEnabled()
      expect(submitButton()).toHaveAttribute('type', 'submit')
    })

    it('does not render any form-level errors', () => {
      setup()

      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    })

    it('renders the button content override when provided', () => {
      setup(serverState(), { buttonContentOverride: 'Resend link' })

      expect(screen.getByRole('button', { name: 'Resend link' })).toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Send link' })).not.toBeInTheDocument()
    })
  })

  describe('with a pre-filled email', () => {
    const email = 'prefilled@example.com'

    it('renders the email as a hidden input instead of a visible field', () => {
      const { container } = render(<ResetPasswordForm email={email} />)

      expect(screen.queryByLabelText('Email address')).not.toBeInTheDocument()

      const hiddenInput = container.querySelector('input[type="hidden"][name="email"]')
      expect(hiddenInput).toHaveValue(email)
    })

    it('submits the pre-filled email without redirecting on success', async () => {
      const { user } = setup(successState(), { email, buttonContentOverride: 'Resend link' })

      await user.click(screen.getByRole('button', { name: 'Resend link' }))

      await waitFor(() => expect(mockAction).toHaveBeenCalledTimes(1))

      const [redirectOnSuccess, , formData] = actionCall(0)
      expect(redirectOnSuccess).toBe(false)
      expect(formData.get('email')).toBe(email)
    })
  })

  describe('user input', () => {
    it('updates the input value as the user types', async () => {
      const { user, emailInput } = setup()

      await user.type(emailInput(), 'test@example.com')

      expect(emailInput()).toHaveValue('test@example.com')
    })
  })

  describe('submission', () => {
    it('calls the server action once with the entered email', async () => {
      const { submitWithEmail } = setup()

      await submitWithEmail('test@example.com')

      await waitFor(() => expect(mockAction).toHaveBeenCalledTimes(1))

      const [redirectOnSuccess, , formData] = actionCall(0)
      expect(redirectOnSuccess).toBe(true)
      expect(formData.get('email')).toBe('test@example.com')
    })

    it('does not call the server action when the email is invalid', async () => {
      const { submitWithEmail } = setup()

      await submitWithEmail('not-an-email')

      await waitFor(() => expect(screen.getByLabelText('Email address')).toBeInvalid())
      expect(screen.getByText('Enter a valid email address')).toBeInTheDocument()
      expect(mockAction).not.toHaveBeenCalled()
    })

    it("turns off the browser's own validation so the field shows its error instead", () => {
      setup()

      expect(screen.getByRole('button', { name: 'Send link' }).closest('form')).toHaveAttribute(
        'novalidate',
      )
    })
  })

  describe('onSuccess', () => {
    it('is called exactly once after a successful submission', async () => {
      const onSuccess = vi.fn()
      const { submitWithEmail } = setup(successState(), { onSuccess })

      await submitWithEmail('test@example.com')

      await waitFor(() => expect(onSuccess).toHaveBeenCalledTimes(1))
      expect(mockAction).toHaveBeenCalledTimes(1)
    })

    it('is called again for each subsequent successful submission (resending)', async () => {
      const onSuccess = vi.fn()
      const { user, submitButton } = setup(successState(), {
        email: 'test@example.com',
        onSuccess,
      })

      await user.click(submitButton())
      await waitFor(() => expect(onSuccess).toHaveBeenCalledTimes(1))

      await user.click(submitButton())
      await waitFor(() => expect(onSuccess).toHaveBeenCalledTimes(2))
      expect(mockAction).toHaveBeenCalledTimes(2)
    })

    it('is not called when the server action returns errors', async () => {
      const onSuccess = vi.fn()
      const { submitWithEmail } = setup(serverState({ errors: ['Nope'] } as never), {
        onSuccess,
      })

      await submitWithEmail('test@example.com')

      await waitFor(() => expect(mockAction).toHaveBeenCalledTimes(1))
      await waitFor(() =>
        expect(FormLevelErrors).toHaveBeenLastCalledWith(
          expect.objectContaining({ errors: ['Nope'] }),
          undefined,
        ),
      )
      expect(onSuccess).not.toHaveBeenCalled()
    })

    it('is not called on initial render', () => {
      const onSuccess = vi.fn()
      setup(successState(), { onSuccess })

      expect(onSuccess).not.toHaveBeenCalled()
    })
  })

  describe('field validation', () => {
    it('shows no error state when there is no field error message', () => {
      const { emailInput } = setup()

      expect(emailInput()).not.toHaveAttribute('aria-invalid', 'true')
    })

    it('renders the FormLevelErrors with error', async () => {
      const errors = ['Code has expired', 'Too many attempts']
      const { submitWithEmail } = setup(serverState({ errors } as never))

      await submitWithEmail('test@example.com')

      await waitFor(() =>
        expect(FormLevelErrors).toHaveBeenLastCalledWith(
          expect.objectContaining({ errors }),
          undefined,
        ),
      )
    })
  })
})
