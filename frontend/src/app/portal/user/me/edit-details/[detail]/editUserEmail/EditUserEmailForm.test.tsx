import { initialFormState } from '@tanstack/react-form-nextjs'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { errorMessages } from '@/lib/form/errorMessages'

import EditUserEmailForm from './EditUserEmailForm'
import { updateUserEmailAction } from './serverActions'

// The server action module is marked 'use server', so replace it with a mock.
vi.mock('./serverActions', () => ({
  updateUserEmailAction: vi.fn(),
}))

const mockedAction = vi.mocked(updateUserEmailAction)

const USER_ID = 42
const INITIAL_EMAIL = 'jane@example.com'

const renderForm = (props: Partial<{ userId: number; initialValue: string }> = {}) =>
  render(<EditUserEmailForm userId={USER_ID} initialValue={INITIAL_EMAIL} {...props} />)

const getEmailInput = () => screen.getByRole('textbox', { name: /email address/i })

describe('EditUserDetailForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // useActionState needs the action to resolve to a form state object
    mockedAction.mockResolvedValue(initialFormState as never)
  })

  it('renders the email input pre-filled with the initial value', () => {
    renderForm()

    expect(getEmailInput()).toHaveValue(INITIAL_EMAIL)
    expect(getEmailInput()).toHaveAttribute('autocomplete', 'email')
  })

  it('renders an enabled submit button', () => {
    renderForm()

    expect(screen.getByRole('button', { name: /submit/i })).toBeEnabled()
  })

  it('uses a different initial value when provided', () => {
    renderForm({ initialValue: 'other@example.com' })

    expect(getEmailInput()).toHaveValue('other@example.com')
  })

  it('updates the input as the user types', async () => {
    const user = userEvent.setup()
    renderForm()

    const input = getEmailInput()
    await user.clear(input)
    await user.type(input, 'new@example.com')

    expect(input).toHaveValue('new@example.com')
  })

  it('calls the server action with the user id and the submitted email', async () => {
    const user = userEvent.setup()
    renderForm()

    const input = getEmailInput()
    await user.clear(input)
    await user.type(input, 'new@example.com')
    await user.click(screen.getByRole('button', { name: /submit/i }))

    await waitFor(() => expect(mockedAction).toHaveBeenCalledTimes(1))

    // bound args: (userId, previousState, formData)
    const [userId, , formData] = mockedAction.mock.calls[0] as unknown as [
      number,
      unknown,
      FormData,
    ]
    expect(userId).toBe(USER_ID)
    expect(formData.get('value')).toBe('new@example.com')
  })

  it('submits the initial value if the user does not change it', async () => {
    const user = userEvent.setup()
    renderForm()

    await user.click(screen.getByRole('button', { name: /submit/i }))

    await waitFor(() => expect(mockedAction).toHaveBeenCalledTimes(1))

    const formData = mockedAction.mock.calls[0][2] as unknown as FormData
    expect(formData.get('value')).toBe(INITIAL_EMAIL)
  })

  it('displays a server validation error returned by the action', async () => {
    const user = userEvent.setup()
    const errorMessage = 'Enter a valid email address'

    // Adjust this shape to match what your server action's validation returns.
    mockedAction.mockResolvedValue({
      ...initialFormState,
      errorMap: { onServer: { fields: { value: errorMessage } } },
      values: { value: 'not-an-email' },
    } as never)

    renderForm()

    await user.click(screen.getByRole('button', { name: /submit/i }))

    expect(await screen.findByText(errorMessage)).toBeInTheDocument()
  })

  it('keeps the submitted value in the input after a server error', async () => {
    const user = userEvent.setup()

    mockedAction.mockResolvedValue({
      ...initialFormState,
      errorMap: { onServer: { fields: { value: 'Enter a valid email address' } } },
      values: { value: 'not-an-email' },
    } as never)

    renderForm()

    const input = getEmailInput()
    await user.clear(input)
    await user.type(input, 'not-an-email')
    await user.click(screen.getByRole('button', { name: /submit/i }))

    await screen.findByText(errorMessages.emailFormat)
    expect(input).toHaveValue('not-an-email')
  })
})
