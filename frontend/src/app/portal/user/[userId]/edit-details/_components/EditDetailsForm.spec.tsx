import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { EditDetailsForm } from './EditDetailsForm'

const mocks = vi.hoisted(() => ({
  back: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    back: mocks.back,
  }),
}))

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

const requiredErrors = [
  { label: 'Full name', message: 'Enter your full name' },
  { label: 'Work email address', message: 'Enter your work email address' },
  { label: 'Contact number', message: 'Enter your phone number' },
]

function renderForm() {
  render(<EditDetailsForm />)
}

function setFieldValue(label: string, value: string) {
  fireEvent.change(screen.getByLabelText(label), { target: { value } })
}

function clearForm(value = '') {
  for (const { label } of requiredErrors) {
    setFieldValue(label, value)
  }
}

function fillValidForm() {
  setFieldValue('Full name', 'Test User')
  setFieldValue('Work email address', 'test@example.com')
  setFieldValue('Contact number', '01234567890')
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

  it('shows required errors and associates them with invalid inputs', async () => {
    renderForm()

    clearForm()
    fireEvent.click(screen.getByRole('button', { name: 'Save' }))

    for (const { message } of requiredErrors) {
      expect(await screen.findByText(message)).toBeDefined()
    }

    const expectedDescriptions = new Map([
      ['Full name', 'fullName-error'],
      ['Work email address', 'workEmail-error'],
      ['Contact number', 'phoneNumber-hint phoneNumber-error'],
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
    fireEvent.click(screen.getByRole('button', { name: 'Save' }))

    for (const { message } of requiredErrors) {
      expect(await screen.findByText(message)).toBeDefined()
    }
  })

  it('shows an email format error for an invalid work email address', async () => {
    renderForm()

    fillValidForm()
    setFieldValue('Work email address', 'not-an-email-address')
    fireEvent.click(screen.getByRole('button', { name: 'Save' }))

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
    fireEvent.click(screen.getByRole('button', { name: 'Save' }))

    await waitFor(() => {
      for (const { message } of requiredErrors) {
        expect(screen.queryByText(message)).toBeNull()
      }
      expect(
        screen.queryByText('Enter an email address in the correct format, like name@example.com'),
      ).toBeNull()
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
