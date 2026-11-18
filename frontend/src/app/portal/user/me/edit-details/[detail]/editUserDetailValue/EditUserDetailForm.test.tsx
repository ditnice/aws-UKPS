import { initialFormState } from '@tanstack/react-form-nextjs'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useActionState } from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { EditableUserDetail } from '@/client/generated'

import EditUserDetailForm from './EditUserDetailForm'
import { updateUserDetailsAction } from './serverActions'

// --- Mocks -----------------------------------------------------------------

// Control the server-action state that the component receives
vi.mock('react', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react')>()
  return { ...actual, useActionState: vi.fn() }
})

vi.mock('./serverActions', () => ({
  updateUserDetailsAction: vi.fn(),
}))

// Keep the form options minimal and self-contained
vi.mock('./shared', () => ({
  updateUserDetailOptions: {
    defaultValues: { property: '', value: '' },
  },
}))

vi.mock('@/lib/form/getFieldErrorMessage', () => ({
  getFieldErrorMessage: (errors: unknown[] = []) => (errors.length ? String(errors[0]) : undefined),
}))

// Lightweight stand-ins for the design-system components
vi.mock('@/components/Input/Input', () => ({
  Input: ({
    label,
    name,
    value,
    onChange,
    onBlur,
    error,
    errorMessage,
    type,
  }: {
    label: string
    name: string
    value: string
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void
    onBlur: () => void
    error?: boolean
    errorMessage?: string
    type: string
  }) => (
    <div>
      <label htmlFor={name}>{label}</label>
      <input
        id={name}
        name={name}
        type={type}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        aria-invalid={error}
      />
      {error && <span role="alert">{errorMessage}</span>}
    </div>
  ),
}))

vi.mock('@/components/Button/Button', () => ({
  Button: ({
    children,
    variant,
    ...props
  }: React.ButtonHTMLAttributes<HTMLButtonElement> & { variant?: string }) => (
    <button data-variant={variant} {...props}>
      {children}
    </button>
  ),
}))

// --- Helpers ---------------------------------------------------------------

const USER_ID = 42
const PROPERTY = 'first_name' as unknown as EditableUserDetail
const LABEL = 'First name'

const mockedUseActionState = vi.mocked(useActionState)
const mockedServerAction = vi.mocked(updateUserDetailsAction)

const renderForm = (overrides: Partial<React.ComponentProps<typeof EditUserDetailForm>> = {}) =>
  render(
    <EditUserDetailForm
      userId={USER_ID}
      initialValue="Alice"
      property={PROPERTY}
      label={LABEL}
      {...overrides}
    />,
  )

describe('EditUserDetailForm', () => {
  const formAction = vi.fn()

  beforeEach(() => {
    // The component has debug console.logs; keep test output clean
    vi.spyOn(console, 'log').mockImplementation(() => {})
    mockedUseActionState.mockReturnValue([initialFormState, formAction, false] as never)
  })

  afterEach(() => {
    vi.clearAllMocks()
    vi.restoreAllMocks()
  })

  describe('rendering', () => {
    it('renders the text input with the given label and initial value', () => {
      renderForm()

      const input = screen.getByLabelText(LABEL)
      expect(input).toBeInTheDocument()
      expect(input).toHaveAttribute('type', 'text')
      expect(input).toHaveValue('Alice')
    })

    it('renders a hidden input carrying the property name', () => {
      const { container } = renderForm()

      const hidden = container.querySelector('input[type="hidden"][name="property"]')
      expect(hidden).toBeInTheDocument()
      expect(hidden).toHaveValue(PROPERTY)
    })

    it('renders an enabled CTA submit button', () => {
      renderForm()

      const button = screen.getByRole('button', { name: /save/i })
      expect(button).toHaveAttribute('type', 'submit')
      expect(button).toHaveAttribute('data-variant', 'cta')
      expect(button).toBeEnabled()
    })

    it('does not show an error by default', () => {
      renderForm()

      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
      expect(screen.getByLabelText(LABEL)).not.toHaveAttribute('aria-invalid', 'true')
    })
  })

  describe('server action wiring', () => {
    it('passes the initial form state to useActionState', () => {
      renderForm()

      expect(mockedUseActionState).toHaveBeenCalledWith(expect.any(Function), initialFormState)
    })

    it('binds the userId as the first argument of the server action', async () => {
      renderForm()

      const boundAction = mockedUseActionState.mock.calls[0][0] as (...args: unknown[]) => unknown
      const formData = new FormData()
      await boundAction(initialFormState, formData)

      expect(mockedServerAction).toHaveBeenCalledWith(USER_ID, initialFormState, formData)
    })
  })

  describe('user interaction', () => {
    it('updates the input value as the user types', async () => {
      const user = userEvent.setup()
      renderForm()

      const input = screen.getByLabelText(LABEL)
      await user.clear(input)
      await user.type(input, 'Bob')

      expect(input).toHaveValue('Bob')
    })

    it('submits the form action with the property and edited value', async () => {
      const user = userEvent.setup()
      renderForm()

      const input = screen.getByLabelText(LABEL)
      await user.clear(input)
      await user.type(input, 'Bob')
      await user.click(screen.getByRole('button', { name: /save/i }))

      await waitFor(() => expect(formAction).toHaveBeenCalledTimes(1))

      const formData = formAction.mock.calls[0][0] as FormData
      expect(formData.get('property')).toBe(PROPERTY)
      expect(formData.get('value')).toBe('Bob')
    })

    it('submits the initial value when the user does not change anything', async () => {
      const user = userEvent.setup()
      renderForm()

      await user.click(screen.getByRole('button', { name: /save/i }))

      await waitFor(() => expect(formAction).toHaveBeenCalledTimes(1))
      const formData = formAction.mock.calls[0][0] as FormData
      expect(formData.get('value')).toBe('Alice')
    })
  })

  describe('server state merging', () => {
    it('shows values returned from the server action (e.g. after a failed submit)', () => {
      mockedUseActionState.mockReturnValue([
        { ...initialFormState, values: { property: PROPERTY, value: 'Server value' } },
        formAction,
        false,
      ] as never)

      renderForm()

      expect(screen.getByLabelText(LABEL)).toHaveValue('Server value')
    })

    it('always keeps the property prop, even if the server state has a different one', () => {
      mockedUseActionState.mockReturnValue([
        { ...initialFormState, values: { property: 'something_else', value: 'x' } },
        formAction,
        false,
      ] as never)

      const { container } = renderForm()

      const hidden = container.querySelector('input[type="hidden"][name="property"]')
      expect(hidden).toHaveValue(PROPERTY)
    })

    it('displays a field error returned from the server', () => {
      // NOTE: adjust to match the exact ServerFormState shape your server action returns
      mockedUseActionState.mockReturnValue([
        {
          ...initialFormState,
          values: { property: PROPERTY, value: '' },
          errorMap: { onServer: { fields: { value: 'Value is required' } } },
          errors: [],
        },
        formAction,
        false,
      ] as never)

      renderForm()

      expect(screen.getByRole('alert')).toHaveTextContent('Value is required')
      expect(screen.getByLabelText(LABEL)).toHaveAttribute('aria-invalid', 'true')
    })
  })
})
