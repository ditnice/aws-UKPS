import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { UpdateUserDetailsCommand } from '@/client/generated'
import { fakeUpdateUserDetailsCommand } from '@/client/generated/@faker-js/faker.gen'
import { errorMessages } from '@/lib/form/errorMessages'

import { EditDetailsForm, EditDetailsFormProps } from './EditDetailsForm'

const mocks = vi.hoisted(() => ({
  back: vi.fn(),
  push: vi.fn(),
  patchUsersByUserId: vi.fn(),
  phoneNumberValidationMock: vi.fn(),
}))

vi.mock('libphonenumber-js/max', () => ({
  isValidPhoneNumber: mocks.phoneNumberValidationMock,
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    back: mocks.back,
    push: mocks.push,
  }),
}))

vi.mock('@/client/generated', () => ({
  patchUsersByUserId: mocks.patchUsersByUserId,
}))

mocks.patchUsersByUserId.mockResolvedValue({
  response: {
    ok: true,
  },
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

beforeEach(() => {
  mocks.phoneNumberValidationMock.mockReturnValue(true)
})

const requiredErrors = [
  { label: 'Full name', message: 'Enter your full name' },
  { label: 'Work email address', message: 'Enter your work email address' },
  { label: 'Contact number', message: 'Enter your phone number' },
]

function renderForm(override?: Partial<EditDetailsFormProps>) {
  const defaults: EditDetailsFormProps = {
    userId: 123,
    initialValues: fakeUpdateUserDetailsCommand(),
  }
  const props = { ...defaults, ...override }
  render(<EditDetailsForm userId={props.userId} initialValues={props.initialValues} />)
}

function setFieldValue(label: string, value: string) {
  fireEvent.change(screen.getByLabelText(label), { target: { value } })
}

function clearForm(value = '') {
  for (const { label } of requiredErrors) {
    setFieldValue(label, value)
  }
}

const validFormValues = {
  fullName: 'Test User',
  workEmail: 'test@example.com',
  workTelephone: '01234567890',
}

function fillValidForm() {
  updateForm(validFormValues)
}

function updateForm(validRequest: UpdateUserDetailsCommand) {
  setFieldValue('Full name', validRequest.fullName)
  setFieldValue('Work email address', validRequest.workEmail)
  setFieldValue('Contact number', validRequest.workTelephone)
}

function clickSubmit() {
  fireEvent.click(screen.getByRole('button', { name: 'Save' }))
}

describe('EditDetailsForm', () => {
  it('renders the edit details controls with appropriate input semantics', () => {
    renderForm()

    const fullName = screen.getByLabelText('Full name')
    expect(fullName.getAttribute('type')).toBe('text')
    expect(fullName.getAttribute('autocomplete')).toBe('name')

    const workEmail = screen.getByLabelText('Work email address')
    expect(workEmail.getAttribute('type')).toBe('email')
    expect(workEmail.getAttribute('autocomplete')).toBe('email')

    const contactNumber = screen.getByLabelText('Contact number')
    expect(contactNumber.getAttribute('type')).toBe('tel')
    expect(contactNumber.getAttribute('autocomplete')).toBe('tel')
    expect(screen.getByText('For international numbers include the country code.')).toBeDefined()

    expect(screen.getByRole('button', { name: 'Save' }).getAttribute('type')).toBe('submit')
    expect(screen.getByRole('button', { name: 'Cancel' }).getAttribute('type')).toBe('button')
  })

  it('does not validate fields on blur before the first submission', () => {
    renderForm()

    clearForm()
    for (const { label } of requiredErrors) {
      fireEvent.blur(screen.getByLabelText(label))
    }

    for (const { message } of requiredErrors) {
      expect(screen.queryByText(message)).toBeNull()
    }
  })

  it('validates the phone number as a valid phone number', async () => {
    mocks.phoneNumberValidationMock.mockReturnValue(false)

    const examplePhoneNumber = '63846484638'
    renderForm()
    updateForm({ ...validFormValues, workTelephone: examplePhoneNumber })
    clickSubmit()

    await waitFor(async () => {
      expect(mocks.phoneNumberValidationMock).toHaveBeenCalledWith(examplePhoneNumber, 'GB')
      expect(await screen.findByText(errorMessages.phoneFormat)).toBeDefined()
    })
  })

  it('shows required errors and associates them with invalid inputs', async () => {
    renderForm()
    clearForm()
    clickSubmit()

    for (const { message } of requiredErrors) {
      expect(await screen.findByText(message)).toBeDefined()
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

    clearForm('   ')
    clickSubmit()

    for (const { message } of requiredErrors) {
      expect(await screen.findByText(message)).toBeDefined()
    }
  })

  it('shows an email format error for an invalid work email address', async () => {
    renderForm()

    fillValidForm()
    setFieldValue('Work email address', 'not-an-email-address')
    clickSubmit()

    expect(
      await screen.findByText(
        'Enter an email address in the correct format, like name@example.com',
      ),
    ).toBeDefined()
    expect(screen.queryByText('Enter your full name')).toBeNull()
    expect(screen.queryByText('Enter your phone number')).toBeNull()
  })

  it('revalidates an invalid field on blur after submission', async () => {
    renderForm()

    clearForm()
    fireEvent.click(screen.getByRole('button', { name: 'Save' }))

    expect(await screen.findByText('Enter your full name')).toBeDefined()

    setFieldValue('Full name', 'Test User')
    expect(screen.getByText('Enter your full name')).toBeDefined()

    fireEvent.blur(screen.getByLabelText('Full name'))

    await waitFor(() => {
      expect(screen.queryByText('Enter your full name')).toBeNull()
    })
    expect(screen.getByLabelText('Full name').getAttribute('aria-invalid')).toBeNull()
    expect(screen.getByLabelText('Full name').getAttribute('aria-describedby')).toBeNull()
  })

  it('does not show validation errors for valid values', async () => {
    renderForm()

    fillValidForm()
    clickSubmit()

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
    updateForm(validRequest)
    clickSubmit()

    await waitFor(() => {
      expect(mocks.patchUsersByUserId).toHaveBeenCalledWith({
        path: { userId: exampleUserId },
        body: validRequest,
      })
    })
  })

  it('forwards you to your current user details on success', async () => {
    renderForm()
    fillValidForm()
    clickSubmit()
    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledWith('/portal/user/me')
    })
  })

  it('navigates back without submitting when Cancel is selected', () => {
    renderForm()

    clearForm()

    const cancel = screen.getByRole('button', { name: 'Cancel' })
    expect(cancel.getAttribute('type')).toBe('button')

    fireEvent.click(cancel)

    expect(mocks.back).toHaveBeenCalledOnce()
    for (const { message } of requiredErrors) {
      expect(screen.queryByText(message)).toBeNull()
    }
  })
})
