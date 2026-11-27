import { initialFormState } from '@tanstack/react-form-nextjs'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useActionState } from 'react'
import { renderToString } from 'react-dom/server'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import FormLevelErrors from '@/lib/form/FormLevelErrors'

import { confirmResetPasswordAction } from './confirmResetPasswordAction'
import ConfirmResetPasswordForm from './ConfirmResetPasswordForm'
import { confirmPasswordResetFormOptions } from './confirmResetPasswordShared'

// Server action can't run in jsdom, so mock it
vi.mock('./confirmResetPasswordAction', () => ({
  confirmResetPasswordAction: vi.fn(),
}))

// Keep the real React, but make useActionState controllable
vi.mock('react', async (importOriginal) => ({
  ...(await importOriginal<typeof import('react')>()),
  useActionState: vi.fn(),
}))

vi.mock('@/lib/form/FormLevelErrors', () => ({
  default: vi.fn(() => null),
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

    it('renders the FormLevelErrors with error', () => {
      const errors = ['Code has expired', 'Too many attempts']
      mockActionState({ errors } as never)
      setup()

      expect(FormLevelErrors).toHaveBeenCalledWith({ errors }, undefined)
    })
  })

  describe('server errors', () => {
    it('does not render server errors when there are none', () => {
      mockActionState({ errors: undefined } as never)

      setup()

      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    })
  })

  describe('server field errors', () => {
    const codeError = 'The confirmation code is incorrect'
    const passwordError = 'The new password does not meet the password requirements'

    beforeEach(() => {
      mockActionState({
        values: { ...defaultValues, confirmationCode: '123456', newPassword: 'Passw0rd!' },
        errorMap: {
          onServer: {
            fields: { confirmationCode: [codeError], newPassword: [passwordError] },
          },
        },
      } as never)
    })

    it('shows each error on its own field', () => {
      setup()

      expect(screen.getByText(codeError)).toBeInTheDocument()
      expect(screen.getByText(passwordError)).toBeInTheDocument()
    })

    it('shows them in the server-rendered page, for when JavaScript is off', () => {
      const html = renderToString(<ConfirmResetPasswordForm {...props} />)

      expect(html).toContain(codeError)
      expect(html).toContain(passwordError)
    })

    it('keeps an error when its field is only focused and left', async () => {
      const { user } = setup()

      await user.click(getInput('confirmationCode'))
      await user.tab()

      expect(screen.getByText(codeError)).toBeInTheDocument()
    })

    it('does not let an earlier server error stop the next submission', async () => {
      const { user } = setup()

      await user.click(screen.getByRole('button', { name: /Reset password/i }))

      await vi.waitFor(() => expect(mockAction).toHaveBeenCalledOnce())
    })

    it("clears only the edited field's error", async () => {
      const { user } = setup()

      await user.type(getInput('newPassword'), 'x')

      expect(screen.queryByText(passwordError)).not.toBeInTheDocument()
      expect(screen.getByText(codeError)).toBeInTheDocument()
    })
  })

  describe('submitting', () => {
    it('shows client validation errors without calling the action', async () => {
      const { user } = setup()

      await user.type(getInput('confirmationCode'), '123')
      await user.click(screen.getByRole('button', { name: /Reset password/i }))

      expect(
        await screen.findByText('Confirmation code must be 6 characters long'),
      ).toBeInTheDocument()
      expect(mockAction).not.toHaveBeenCalled()
    })

    it('calls the action with the form data when the values are valid', async () => {
      const { user } = setup()

      await user.type(getInput('confirmationCode'), '123456')
      await user.type(getInput('newPassword'), 'Passw0rd!')
      await user.click(screen.getByRole('button', { name: /Reset password/i }))

      await vi.waitFor(() => expect(mockAction).toHaveBeenCalledOnce())
      const formData = mockAction.mock.calls[0][0] as FormData
      expect(Object.fromEntries(formData)).toEqual({
        email: props.email,
        confirmationCode: '123456',
        newPassword: 'Passw0rd!',
      })
    })
  })

  describe('submit button', () => {
    it('renders an enabled submit button', () => {
      setup()

      const button = screen.getByRole('button', { name: /Reset password/i })

      expect(button).toHaveAttribute('type', 'submit')
      expect(button).toBeEnabled()
    })
  })
})
