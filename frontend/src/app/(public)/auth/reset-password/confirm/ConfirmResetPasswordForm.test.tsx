import { initialFormState } from '@tanstack/react-form-nextjs'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useActionState } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { confirmResetPasswordAction } from './confirmResetPasswordAction'
import ConfirmResetPasswordForm from './ConfirmResetPasswordForm'
import { confirmPasswordResetFormOptions } from './confirmResetPasswordShared'

// Server action can't run in jsdom, so mock it
vi.mock('./confirmResetPasswordAction', () => ({
  confirmResetPasswordAction: vi.fn(),
}))

// Mock the error helper so we can drive field errors without building TanStack server state
vi.mock('@/lib/form/getFieldErrorMessage', () => ({
  getFieldErrorMessage: vi.fn(),
}))

// Keep the real React, but make useActionState controllable
vi.mock('react', async (importOriginal) => ({
  ...(await importOriginal<typeof import('react')>()),
  useActionState: vi.fn(),
}))

const { defaultValues } = confirmPasswordResetFormOptions

const props = { email: 'user@example.com' }

const fieldNames = Object.keys(defaultValues) as Array<keyof typeof defaultValues>
const visibleFields = fieldNames.filter((name) => name !== 'email')

// Single source of truth for how each field is found in the DOM
const labels: Record<(typeof visibleFields)[number], RegExp> = {
  confirmationCode: /confirmation code/i,
  newPassword: /new password/i,
}

const mockAction = vi.fn()

const mockActionState = (state: Partial<typeof initialFormState> = {}) =>
  vi
    .mocked(useActionState)
    .mockReturnValue([{ ...initialFormState, ...state }, mockAction, false] as never)

const setup = () => {
  const user = userEvent.setup()
  const utils = render(<ConfirmResetPasswordForm {...props} />)
  return { user, ...utils }
}

const getInput = (name: keyof typeof labels) => screen.getByLabelText(labels[name])

describe('ConfirmResetPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockActionState()
    vi.mocked(getFieldErrorMessage).mockReturnValue(undefined as never)
  })

  describe('wiring', () => {
    it('uses the confirm reset password action with the initial form state', () => {
      setup()

      expect(useActionState).toHaveBeenCalledWith(confirmResetPasswordAction, initialFormState)
    })
  })

  describe('fields', () => {
    it('renders a hidden email field containing the email prop', () => {
      const { container } = setup()

      const hidden = container.querySelector<HTMLInputElement>('input[type="hidden"][name="email"]')

      expect(hidden).toBeInTheDocument()
      expect(hidden).toHaveValue(props.email)
    })

    it.each(visibleFields)('renders the %s field with its name and default value', (name) => {
      setup()

      const input = getInput(name)

      expect(input).toHaveAttribute('name', name)
      expect(input).toHaveValue(defaultValues[name])
    })

    it.each(visibleFields)('updates the %s field as the user types', async (name) => {
      const value = `typed-${name}`
      const { user } = setup()

      await user.type(getInput(name), value)

      expect(getInput(name)).toHaveValue(value)
    })

    it.each(visibleFields)('prefills the %s field from the action state values', (name) => {
      const value = `from-state-${name}`
      mockActionState({ values: { ...defaultValues, [name]: value } } as never)

      setup()

      expect(getInput(name)).toHaveValue(value)
    })

    it('always uses the email prop even if state values contain a different email', () => {
      mockActionState({ values: { ...defaultValues, email: 'other@example.com' } } as never)

      const { container } = setup()

      expect(container.querySelector('input[name="email"]')).toHaveValue(props.email)
    })
  })

  describe('validation messages', () => {
    it('shows no field error messages by default', () => {
      setup()

      expect(screen.queryByText(/error/i)).not.toBeInTheDocument()
    })

    it.each(visibleFields)('shows the error message for the %s field', (name) => {
      const message = `${name} is invalid`
      vi.mocked(getFieldErrorMessage).mockReturnValue(message as never)

      setup()

      // Every visible field shares the mocked helper, so each renders the message
      expect(screen.getAllByText(message)).toHaveLength(visibleFields.length)
    })
  })

  describe('server errors', () => {
    it('does not render server errors when there are none', () => {
      mockActionState({ errors: undefined } as never)

      setup()

      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    })

    it('renders each server error from the action state', () => {
      const errors = ['Code has expired', 'Too many attempts']
      mockActionState({ errors } as never)

      setup()

      errors.forEach((error) => expect(screen.getByText(error)).toBeInTheDocument())
    })
  })

  describe('submit button', () => {
    it('renders an enabled submit button', () => {
      setup()

      const button = screen.getByRole('button', { name: /send link/i })

      expect(button).toHaveAttribute('type', 'submit')
      expect(button).toBeEnabled()
    })
  })
})
