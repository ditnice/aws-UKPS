import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { UpdateUserDetailsCommand } from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'
import { router } from '@/test-utils/nextNavigation'
import { fillInput } from '@/test-utils/userInteractions'

import { EditDetailsForm, EditDetailsFormProps } from './EditDetailsForm'

const mocks = vi.hoisted(() => ({
  patchUsersByUserId: vi.fn(),
  phoneNumberValidationMock: vi.fn(),
  buildSignInHref: vi.fn(),
  buildEditDetailsHref: vi.fn(),
}))

vi.mock('libphonenumber-js/max', () => ({
  isValidPhoneNumber: mocks.phoneNumberValidationMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  patchUsersByUserId: mocks.patchUsersByUserId,
}))

vi.mock('@/lib/auth/routing', () => ({
  buildSignInHref: mocks.buildSignInHref,
}))

vi.mock('../navigation', () => ({ buildEditDetailsHref: mocks.buildEditDetailsHref }))

beforeEach(() => {
  mocks.phoneNumberValidationMock.mockReturnValue(true)
  mocks.patchUsersByUserId.mockReset()
  mocks.patchUsersByUserId.mockResolvedValue({
    response: { ok: true },
  })
})

const requiredErrors = [
  { label: 'Full name', message: 'Enter your full name' },
  { label: 'Work email address', message: 'Enter your work email address' },
  { label: 'Contact number', message: 'Enter your phone number' },
]

function renderForm(override?: Partial<EditDetailsFormProps>) {
  const defaults: EditDetailsFormProps = {
    userId: 123,
    initialValues: { ...validFormValues },
    sessionKey: undefined,
  }
  const props = { ...defaults, ...override }
  render(<EditDetailsForm {...props} />)
}

async function setFieldValue(label: string, value: string) {
  await fillInput(user, screen.getByLabelText(label), value)
}

async function clearForm(value = '') {
  for (const { label } of requiredErrors) {
    await setFieldValue(label, value)
  }
}

const validFormValues = {
  fullName: 'Test User',
  workEmail: 'test@example.com',
  workTelephone: '01234567890',
} satisfies UpdateUserDetailsCommand

async function fillValidForm() {
  await updateForm(validFormValues)
}

async function updateForm(validRequest: UpdateUserDetailsCommand) {
  await setFieldValue('Full name', validRequest.fullName)
  await setFieldValue('Work email address', validRequest.workEmail)
  await setFieldValue('Contact number', validRequest.workTelephone)
}

async function clickSubmit() {
  await user.click(screen.getByRole('button', { name: 'Save' }))
}

async function clickSignInAgainButton() {
  const button = screen.getByRole('button', { name: 'Sign in again' })
  await user.click(button)
}

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('EditDetailsForm', () => {
  it('renders the edit details controls with appropriate input semantics', () => {
    renderForm()

    const fullName = screen.getByLabelText('Full name')
    expect(fullName).toHaveValue(validFormValues.fullName)
    expect(fullName.getAttribute('type')).toBe('text')
    expect(fullName.getAttribute('autocomplete')).toBe('name')

    const workEmail = screen.getByLabelText('Work email address')
    expect(workEmail).toHaveValue(validFormValues.workEmail)
    expect(workEmail.getAttribute('type')).toBe('email')
    expect(workEmail.getAttribute('autocomplete')).toBe('email')

    const contactNumber = screen.getByLabelText('Contact number')
    expect(contactNumber).toHaveValue(validFormValues.workTelephone)
    expect(contactNumber.getAttribute('type')).toBe('tel')
    expect(contactNumber.getAttribute('autocomplete')).toBe('tel')
    expect(
      screen.getByText(
        'This number will be visible to horizon scanning organisations. For international numbers include the country code.',
      ),
    ).toBeInTheDocument()

    expect(screen.getByRole('button', { name: 'Save' }).getAttribute('type')).toBe('submit')
    expect(screen.getByRole('button', { name: 'Cancel' }).getAttribute('type')).toBe('button')
  })

  it('does not validate fields on blur before the first submission', async () => {
    renderForm()

    await clearForm()
    for (const { label } of requiredErrors) {
      await user.click(screen.getByLabelText(label))
      await user.tab()
    }

    for (const { message } of requiredErrors) {
      expect(screen.queryByText(message)).toBeNull()
    }
  })

  it('validates the phone number as a valid phone number', async () => {
    mocks.phoneNumberValidationMock.mockReturnValue(false)

    const examplePhoneNumber = '63846484638'
    renderForm()
    await updateForm({ ...validFormValues, workTelephone: examplePhoneNumber })
    await clickSubmit()

    await waitFor(async () => {
      expect(mocks.phoneNumberValidationMock).toHaveBeenCalledWith(examplePhoneNumber, 'GB')
      expect(await screen.findByText(errorMessages.phoneFormat)).toBeInTheDocument()
    })
  })

  it('shows required errors and associates them with invalid inputs', async () => {
    renderForm()
    await clearForm()
    await clickSubmit()

    for (const { message } of requiredErrors) {
      expect(await screen.findByText(message)).toBeInTheDocument()
    }

    const expectedDescriptions = new Map([
      ['Full name', 'fullName-error'],
      ['Work email address', 'workEmail-error'],
      ['Contact number', 'workTelephone-hint workTelephone-error'],
    ])

    for (const [label, description] of expectedDescriptions) {
      const input = screen.getByLabelText(label)
      expect(input.getAttribute('aria-invalid')).toBe('true')
      expect(input.getAttribute('aria-describedby')).toBe(description)

      for (const id of description.split(' ')) {
        expect(document.getElementById(id)).not.toBeNull()
      }
    }
  })

  it('treats whitespace-only values as empty', async () => {
    renderForm()

    await clearForm('   ')
    await clickSubmit()

    for (const { message } of requiredErrors) {
      expect(await screen.findByText(message)).toBeInTheDocument()
    }
  })

  it('shows an email format error for an invalid work email address', async () => {
    renderForm()

    await fillValidForm()
    await setFieldValue('Work email address', 'not-an-email-address')
    await clickSubmit()

    expect(
      await screen.findByText(
        'Enter an email address in the correct format, like name@example.com',
      ),
    ).toBeInTheDocument()
    expect(screen.queryByText('Enter your full name')).toBeNull()
    expect(screen.queryByText('Enter your phone number')).toBeNull()
  })

  it('revalidates an invalid field on blur after submission', async () => {
    renderForm()

    await clearForm()
    await user.click(screen.getByRole('button', { name: 'Save' }))

    expect(await screen.findByText('Enter your full name')).toBeInTheDocument()

    await setFieldValue('Full name', 'Test User')
    expect(screen.getByText('Enter your full name')).toBeInTheDocument()

    await user.tab()

    await waitFor(() => {
      expect(screen.queryByText('Enter your full name')).toBeNull()
    })
    expect(screen.getByLabelText('Full name').getAttribute('aria-invalid')).toBeNull()
    expect(screen.getByLabelText('Full name').getAttribute('aria-describedby')).toBeNull()
  })

  it('does not show validation errors for valid values', async () => {
    renderForm()

    await fillValidForm()
    await clickSubmit()

    await waitFor(() => {
      for (const { message } of requiredErrors) {
        expect(screen.queryByText(message)).toBeNull()
      }
      expect(
        screen.queryByText('Enter an email address in the correct format, like name@example.com'),
      ).toBeNull()
    })
  })

  it('sends command on valid submit', async () => {
    const exampleUserId = 342
    renderForm({ userId: exampleUserId })
    const validRequest = {
      fullName: 'Test User',
      workEmail: 'test@example.com',
      workTelephone: '01234567890',
    }
    await updateForm(validRequest)
    await clickSubmit()

    await waitFor(() => {
      expect(mocks.patchUsersByUserId).toHaveBeenCalledWith({
        path: { userId: exampleUserId },
        body: validRequest,
      })
    })
  })

  it('forwards you to your current user details on success', async () => {
    renderForm()
    await fillValidForm()
    await clickSubmit()
    await waitFor(() => {
      expect(router.push).toHaveBeenCalledWith('/portal/user/me?updated=true')
    })
  })

  it('navigates back without submitting when Cancel is selected', async () => {
    renderForm()

    await clearForm()

    const cancel = screen.getByRole('button', { name: 'Cancel' })
    expect(cancel.getAttribute('type')).toBe('button')

    await user.click(cancel)

    expect(router.back).toHaveBeenCalledOnce()
    for (const { message } of requiredErrors) {
      expect(screen.queryByText(message)).toBeNull()
    }
  })

  it('shows a generic error without navigating when update fails', async () => {
    mocks.patchUsersByUserId.mockResolvedValue({
      response: { ok: false },
      error: { title: 'Update failed' },
    })

    renderForm()
    await fillValidForm()
    await clickSubmit()

    expect(await screen.findByText(errorMessages.updatingUserDetailsError)).toBeInTheDocument()

    expect(router.push).not.toHaveBeenCalled()
    expect(mocks.patchUsersByUserId).toHaveBeenCalledOnce()

    for (const label of ['Full name', 'Work email address', 'Contact number']) {
      expect(screen.getByLabelText(label).getAttribute('aria-invalid')).toBeNull()
    }
  })

  it('shows server validation errors against the corresponding fields', async () => {
    mocks.patchUsersByUserId.mockResolvedValueOnce({
      response: { ok: false },
      error: {
        errors: {
          FullName: ['Server name error'],
          WorkEmail: ['Server email error'],
          WorkTelephone: ['Server telephone error'],
        },
      },
    })

    renderForm()
    await fillValidForm()
    await clickSubmit()

    const expectedErrors = [
      {
        label: 'Full name',
        message: 'Server name error',
        errorId: 'fullName-error',
      },
      {
        label: 'Work email address',
        message: 'Server email error',
        errorId: 'workEmail-error',
      },
      {
        label: 'Contact number',
        message: 'Server telephone error',
        errorId: 'workTelephone-error',
      },
    ]

    for (const { label, message, errorId } of expectedErrors) {
      expect(await screen.findByText(message)).toBeInTheDocument()

      const input = screen.getByLabelText(label)
      expect(input.getAttribute('aria-invalid')).toBe('true')
      expect(input.getAttribute('aria-describedby')?.split(' ')).toContain(errorId)
      expect(document.getElementById(errorId)?.textContent).toContain(message)
    }

    expect(screen.getByText(errorMessages.updatingUserDetailsError)).toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()
  })

  it('shows reauthentication required if ReauthenticationRequired error returned', async () => {
    mocks.patchUsersByUserId.mockResolvedValueOnce({
      response: { ok: false },
      error: {
        code: 'ReauthenticationRequired',
      },
    })

    renderForm()
    await fillValidForm()
    await clickSubmit()

    expect(
      await screen.getByText('You need to sign in again to change your email address'),
    ).toBeInTheDocument()
    expect(await screen.getByRole('button', { name: 'Sign in again' })).toBeInTheDocument()
  })

  it('redirect to sign-in page on `Sign in again` button click', async () => {
    mocks.buildSignInHref.mockReturnValue('/sign-in')
    mocks.patchUsersByUserId.mockResolvedValueOnce({
      response: { ok: false },
      error: {
        code: 'ReauthenticationRequired',
      },
    })

    renderForm()
    await fillValidForm()
    await clickSubmit()

    await clickSignInAgainButton()

    expect(router.push).toHaveBeenCalledWith('/sign-in')
  })

  it('should store state in session storage before redirect', async () => {
    mocks.buildSignInHref.mockReturnValue('/sign-in')
    mocks.patchUsersByUserId.mockResolvedValueOnce({
      response: { ok: false },
      error: {
        code: 'ReauthenticationRequired',
      },
    })

    renderForm()
    await fillValidForm()
    await clickSubmit()

    await clickSignInAgainButton()

    const [args] = mocks.buildEditDetailsHref.mock.calls[0]
    const capturedKey = args!.sessionKey!

    const data = sessionStorage.getItem(capturedKey)

    expect(data).not.toBeNull()
    expect(data).toBe(JSON.stringify(validFormValues))
  })
})
