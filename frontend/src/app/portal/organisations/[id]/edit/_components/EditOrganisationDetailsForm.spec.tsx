import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  ADDRESS_REQUIRED_ERROR_MESSAGE,
  COMPANY_NAME_REQUIRED_ERROR_MESSAGE,
  EMAIL_FORMAT_ERROR_MESSAGE,
  EMAIL_REQUIRED_ERROR_MESSAGE,
  PHONE_FORMAT_ERROR_MESSAGE,
  PHONE_REQUIRED_ERROR_MESSAGE,
} from '@/app/common/ErrorMessages'

import { EditOrganisationDetailsForm } from './EditOrganisationDetailsForm'

import type { EditOrganisationDetailsFormProps } from './EditOrganisationDetailsForm'

const { pushMock, updateOrganisationDetailsActionMock } = vi.hoisted(() => ({
  pushMock: vi.fn(),
  updateOrganisationDetailsActionMock: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: pushMock }),
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
  headOfficeTelephone: '01632 960001',
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
      '01632 960001',
    )
  })

  it('shows a required error for each field left empty on submit, and does not submit', () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company name'), { target: { value: '' } })
    fireEvent.change(screen.getByLabelText('Company address'), { target: { value: '' } })
    fireEvent.change(screen.getByLabelText('Company email address'), { target: { value: '' } })
    fireEvent.change(screen.getByLabelText('Company phone number'), { target: { value: '' } })

    submit()

    expect(screen.getByText(COMPANY_NAME_REQUIRED_ERROR_MESSAGE)).toBeDefined()
    expect(screen.getByText(ADDRESS_REQUIRED_ERROR_MESSAGE)).toBeDefined()
    expect(screen.getByText(EMAIL_REQUIRED_ERROR_MESSAGE)).toBeDefined()
    expect(screen.getByText(PHONE_REQUIRED_ERROR_MESSAGE)).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a format error for an invalid email or phone number, and does not submit', () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Company email address'), {
      target: { value: 'not-an-email' },
    })
    fireEvent.change(screen.getByLabelText('Company phone number'), { target: { value: '123' } })

    submit()

    expect(
      screen.getByText(EMAIL_FORMAT_ERROR_MESSAGE, { selector: '.input__error' }),
    ).toBeDefined()
    expect(
      screen.getByText(PHONE_FORMAT_ERROR_MESSAGE, { selector: '.input__error' }),
    ).toBeDefined()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
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
})
