import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest'

import { errorMessages } from '@/lib/form/errorMessages'
import { router } from '@/test-utils/nextNavigation'
import { fillInput } from '@/test-utils/userInteractions'

import { EditOrganisationDetailsForm } from './EditOrganisationDetailsForm'

import type { EditOrganisationDetailsFormProps } from './EditOrganisationDetailsForm'

const { push: pushMock, back: backMock } = router
const { updateOrganisationDetailsActionMock } = vi.hoisted(() => ({
  updateOrganisationDetailsActionMock: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('../_actions/updateOrganisationDetails', () => ({
  updateOrganisationDetailsAction: updateOrganisationDetailsActionMock,
}))

afterEach(() => {
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

async function submit() {
  await user.click(screen.getByRole('button', { name: 'Submit' }))
}

const organisationNameLabel = 'Organisation name'
const organisationAddressLabel = 'Organisation address'
const organisationEmailLabel = 'Head office email address'
const organisationPhoneNumberLabel = 'Head office phone number'

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('EditOrganisationDetailsForm', () => {
  it('renders each field pre-filled with the given organisation details', () => {
    renderForm()

    expect(screen.getByLabelText(organisationNameLabel)).toHaveValue('Acme Ltd')
    expect(screen.getByLabelText(organisationAddressLabel)).toHaveValue('1 Example Street')
    expect(screen.getByLabelText(organisationEmailLabel)).toHaveValue('contact@example.com')
    expect(screen.getByLabelText(organisationPhoneNumberLabel)).toHaveValue('0121 234 5678')
  })

  it('shows a required error when the organisation name is left empty on submit', async () => {
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationNameLabel), '')
    await submit()

    expect(
      await screen.findByText(errorMessages.organisationNameRequired, {
        selector: '.input__error',
      }),
    ).toBeInTheDocument()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a required error when the organisation address is left empty on submit', async () => {
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationAddressLabel), '')
    await submit()

    expect(
      await screen.findByText(errorMessages.addressRequired, { selector: '.textarea__error' }),
    ).toBeInTheDocument()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a required error when the email address is left empty on submit', async () => {
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationEmailLabel), '')
    await submit()

    expect(
      await screen.findByText(errorMessages.organisationEmailRequired, {
        selector: '.input__error',
      }),
    ).toBeInTheDocument()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a required error when the phone number is left empty on submit', async () => {
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationPhoneNumberLabel), '')
    await submit()

    expect(
      await screen.findByText(errorMessages.phoneRequired, { selector: '.input__error' }),
    ).toBeInTheDocument()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it('shows a format error when the email address is invalid on submit', async () => {
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationEmailLabel), 'not-an-email')
    await submit()

    expect(
      await screen.findByText(errorMessages.emailFormat, { selector: '.input__error' }),
    ).toBeInTheDocument()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it.each([
    'not-a-phone-number', // Non-numeric garbage.
    '123', // Too short, no recognisable structure.
    '01632 960001', // Reserved Ofcom number, rejected by libphonenumber.
    '01 42 68 53 00', // Correct French number, but no country code (parsed as GB).
    '030 83050', // Correct German number, but no country code (parsed as GB).
    '911 234 567', // Correct Spanish number, but no country code (parsed as GB).
    '+999 123 4567', // Non-existent country calling code.
    '+33 0 42 68 53 00', // France, correct length but invalid pattern (trunk 0 kept after country code).
    '020 7946 095890', // UK, valid prefix but too many digits.
    '020-CALL-NOW', // Alphanumeric, not a dialable format.
    '+044 121 234 5678', // Malformed, stray leading zero before country code.
    '+44', // Country code only, no subscriber number.
  ])('shows a format error when the phone number %s is invalid on submit', async (phoneNumber) => {
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationPhoneNumberLabel), phoneNumber)
    await submit()

    expect(
      await screen.findByText(errorMessages.phoneFormat, { selector: '.input__error' }),
    ).toBeInTheDocument()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })

  it.each([
    '020 1234 5678', // UK landline, no country code.
    '07911 123456', // UK mobile, no country code.
    '+44 121 234 5678', // UK, with country code.
    '(020) 1234 5678', // UK, parenthesised area code.
    '020-1234-5678', // UK, hyphenated.
    '020 1234 5678 ext 123', // UK, with extension.
    '+1 (212) 555-0123', // USA.
    '+33 1 42 68 53 00', // France.
    '+49 30 83050', // Germany.
    '+34 91 123 45 67', // Spain.
    '+39 02 3661 8300', // Italy.
    '+31 20 794 0100', // Netherlands.
    '+353 1 234 5678', // Ireland.
    '+351 21 123 4567', // Portugal.
    '+32 470 12 34 56', // Belgium.
    '+46 8 123 456 00', // Sweden.
    '+48 22 123 45 67', // Poland.
    '+45 32 12 34 56', // Denmark.
    '+358 9 123 4567', // Finland.
    '+41 44 668 18 00', // Switzerland.
    '+43 1 234 5678', // Austria.
    '+47 22 12 34 56', // Norway.
  ])('accepts a phone number in the format %s', async (phoneNumber) => {
    updateOrganisationDetailsActionMock.mockResolvedValue({ status: 'success' })
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationPhoneNumberLabel), phoneNumber)
    await submit()

    await waitFor(() => expect(updateOrganisationDetailsActionMock).toHaveBeenCalled())
    expect(screen.queryByText(errorMessages.phoneFormat, { selector: '.input__error' })).toBeNull()
  })

  it('submits the updated values and redirects to the organisation page on success', async () => {
    updateOrganisationDetailsActionMock.mockResolvedValue({ status: 'success' })
    renderForm()

    await fillInput(user, screen.getByLabelText(organisationNameLabel), 'New Name Ltd')
    await submit()

    await waitFor(() =>
      expect(updateOrganisationDetailsActionMock).toHaveBeenCalledWith(1, {
        organisationName: 'New Name Ltd',
        headOfficeAddress: defaultProps.headOfficeAddress,
        headOfficeEmail: defaultProps.headOfficeEmail,
        headOfficeTelephone: defaultProps.headOfficeTelephone,
      }),
    )
    await waitFor(() =>
      expect(pushMock).toHaveBeenCalledWith('/portal/organisations/1?action=updated-details'),
    )
  })

  it('shows the error returned by the server action and does not redirect', async () => {
    updateOrganisationDetailsActionMock.mockResolvedValue({
      status: 'error',
      message: 'There was a problem updating the organisation. Please try again later.',
    })
    renderForm()

    await submit()

    const alert = await screen.findByRole('alert')
    expect(alert.textContent).toBe(
      'There was a problem updating the organisation. Please try again later.',
    )
    expect(pushMock).not.toHaveBeenCalled()
  })

  it('navigates back to the previous page when cancel is clicked', async () => {
    renderForm()

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(backMock).toHaveBeenCalledOnce()
    expect(updateOrganisationDetailsActionMock).not.toHaveBeenCalled()
  })
})
