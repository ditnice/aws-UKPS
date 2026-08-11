import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  ADDRESS_REQUIRED_ERROR_MESSAGE,
  COMPANY_NAME_REQUIRED_ERROR_MESSAGE,
  EMAIL_FORMAT_ERROR_MESSAGE,
  EMAIL_REQUIRED_ERROR_MESSAGE,
  PHONE_FORMAT_ERROR_MESSAGE,
  PHONE_REQUIRED_ERROR_MESSAGE,
} from '@/app/common/form/ErrorMessages'

import { EditOrganisationDetailsForm } from './EditOrganisationDetailsForm'

import type { EditOrganisationDetailsFormProps } from './EditOrganisationDetailsForm'

const { pushMock, backMock, updateOrganisationDetailsActionMock } = vi.hoisted(() => ({
  pushMock: vi.fn(),
  backMock: vi.fn(),
  updateOrganisationDetailsActionMock: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: pushMock, back: backMock }),
}))

vi.mock('../_actions/updateOrganisationDetails', () => ({
  updateOrganisationDetailsAction: updateOrganisationDetailsActionMock,
}))

afterEach(() => {
  cleanup()
  vi.resetAllMocks()
})

const defaultProps: EditOrganisationDetailsFormProps = {
  organisationId: 1,
  organisationName: 'Acme Ltd',
  headOfficeAddress: '1 Example Street',
  headOfficeEmail: 'contact@example.com',
  headOfficeTelephone: '0121 234 5678',
}

function renderForm(overrides: Partial<EditOrganisationDetailsFormProps> = {}) {
  render(<EditOrganisationDetailsForm {...defaultProps} {...overrides} />)
}

function submit() {
  fireEvent.click(screen.getByRole('button', { name: 'Save changes' }))
}

describe('EditOrganisationDetailsForm', () => {
  it('renders each field pre-filled with the given organisation details', () => {
    renderForm()

    expect((screen.getByLabelText('Company name') as HTMLInputElement).value).toBe('Acme Ltd')
    expect((screen.getByLabelText('Company address') as HTMLTextAreaElement).value).toBe(
      '1 Example Street',
    )
    expect((screen.getByLabelText('Company email address') as HTMLInputElement).value).toBe(
      'contact@example.com',
    )
    expect((screen.getByLabelText('Company phone number') as HTMLInputElement).value).toBe(
      '0121 234 5678',
    )
  })

  it('shows a required error when the company name is left empty on submit', async () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company name'), { target: { value: '' } })
    submit()

    expect(
      await screen.findByText(COMPANY_NAME_REQUIRED_ERROR_MESSAGE, { selector: '.input__error' }),
    ).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a required error when the company address is left empty on submit', async () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company address'), { target: { value: '' } })
    submit()

    expect(
      await screen.findByText(ADDRESS_REQUIRED_ERROR_MESSAGE, { selector: '.textarea__error' }),
    ).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a required error when the email address is left empty on submit', async () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company email address'), { target: { value: '' } })
    submit()

    expect(
      await screen.findByText(EMAIL_REQUIRED_ERROR_MESSAGE, { selector: '.input__error' }),
    ).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a required error when the phone number is left empty on submit', async () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company phone number'), { target: { value: '' } })
    submit()

    expect(
      await screen.findByText(PHONE_REQUIRED_ERROR_MESSAGE, { selector: '.input__error' }),
    ).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a format error when the email address is invalid on submit', async () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company email address'), {
      target: { value: 'not-an-email' },
    })
    submit()

    expect(
      await screen.findByText(EMAIL_FORMAT_ERROR_MESSAGE, { selector: '.input__error' }),
    ).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a format error when the phone number is invalid on submit', async () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company phone number'), { target: { value: '123' } })
    submit()

    expect(
      await screen.findByText(PHONE_FORMAT_ERROR_MESSAGE, { selector: '.input__error' }),
    ).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it.each([
    '0121 234 5678',
    '(020) 1234 5678',
    '020-1234-5678',
    '020 1234 5678 ext 123',
    '+1 (212) 555-0123',
  ])('accepts a phone number in the format %s', async (phoneNumber) => {
    updateOrganisationDetailsActionMock.mockResolvedValue({ status: 'success' })
    renderForm()

    fireEvent.change(screen.getByLabelText('Company phone number'), {
      target: { value: phoneNumber },
    })
    submit()

    await waitFor(() => expect(updateOrganisationDetailsActionMock).toHaveBeenCalled())
    expect(screen.queryByText(PHONE_FORMAT_ERROR_MESSAGE, { selector: '.input__error' })).toBeNull()
  })

  it('submits the updated values and redirects to the organisation page on success', async () => {
    updateOrganisationDetailsActionMock.mockResolvedValue({ status: 'success' })
    renderForm()

    fireEvent.change(screen.getByLabelText('Company name'), { target: { value: 'New Name Ltd' } })
    submit()

    await waitFor(() =>
      expect(updateOrganisationDetailsActionMock).toHaveBeenCalledWith(1, {
        organisationName: 'New Name Ltd',
        headOfficeAddress: defaultProps.headOfficeAddress,
        headOfficeEmail: defaultProps.headOfficeEmail,
        headOfficeTelephone: defaultProps.headOfficeTelephone,
      }),
    )
    await waitFor(() => expect(pushMock).toHaveBeenCalledWith('/portal/organisations/1'))
  })

  it('shows the error returned by the server action and does not redirect', async () => {
    updateOrganisationDetailsActionMock.mockResolvedValue({
      status: 'error',
      message: 'There was a problem updating the organisation. Please try again later.',
    })
    renderForm()

    submit()

    const alert = await screen.findByRole('alert')
    expect(alert.textContent).toBe(
      'There was a problem updating the organisation. Please try again later.',
    )
    expect(pushMock).not.toHaveBeenCalled()
  })

  it('navigates back to the previous page when cancel is clicked', () => {
    renderForm()

    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(backMock).toHaveBeenCalledOnce()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })
})
