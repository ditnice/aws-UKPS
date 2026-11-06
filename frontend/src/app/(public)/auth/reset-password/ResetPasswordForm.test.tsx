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

/** Builds the state the server action would return, merged over the initial state. */
const serverState = (overrides: Partial<FormState> = {}): FormState =>
  ({ ...initialFormState, ...overrides }) as FormState

/** Renders the form and returns the elements/helpers every test needs. */
const setup = (actionResult: FormState = serverState()) => {
  mockAction.mockResolvedValue(actionResult as never)

  const user = userEvent.setup()
  render(<ResetPasswordForm />)

  const emailInput = () => screen.getByLabelText('Email address')
  const submitButton = () => screen.getByRole('button', { name: 'Send link' })

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
  })

  describe('user input', () => {
    it('updates the input value as the user types', async () => {
      const { user, emailInput } = setup()

      await user.type(emailInput(), 'test@example.com')

      expect(emailInput()).toHaveValue('test@example.com')
    })
  })

  describe('submission', () => {
    it('calls the server action with the entered email', async () => {
      const { submitWithEmail } = setup()

      await submitWithEmail('test@example.com')

      await waitFor(() => expect(mockAction).toHaveBeenCalledTimes(1))

      const [, formData] = mockAction.mock.calls[0] as unknown as [unknown, FormData]
      expect(formData.get('email')).toBe('test@example.com')
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
          undefined, // drop this arg if you're on React 19
        ),
      )
    })
  })
})
