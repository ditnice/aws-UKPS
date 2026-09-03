import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { OnboardUserCommandDto } from '@/client/generated'
import { postUsersOnboard } from '@/client/generated/sdk.gen'
import { errorMessages } from '@/lib/form/errorMessages'
import { NextLinkMock } from '@/test-utils/nextMocks'

import { OrganisationOnboardUserForm } from './OrganisationOnboardUserForm'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  phoneNumberValidationMock: vi.fn(),
}))

vi.mock('libphonenumber-js/max', () => ({
  isValidPhoneNumber: mocks.phoneNumberValidationMock,
}))

vi.mock('@/client/generated/sdk.gen', () => ({
  postUsersOnboard: vi.fn(),
}))

vi.mock('next/link', () => ({
  default: NextLinkMock,
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mocks.push,
  }),
}))

beforeEach(() => {
  mocks.phoneNumberValidationMock.mockReturnValue(true)
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

type FormValues = Omit<OnboardUserCommandDto, 'organisationId'>
const validFormValues: FormValues = {
  fullName: 'Test User',
  newUserEmail: 'test@test.com',
  contactNumber: '01234567890',
}

function renderForm() {
  render(<OrganisationOnboardUserForm organisationId={123} />)
}

function enterValuesIntoForm(validFormValues: FormValues) {
  fireEvent.change(screen.getByLabelText('Full name'), {
    target: { value: validFormValues.fullName },
  })
  fireEvent.change(screen.getByLabelText('Work email address'), {
    target: { value: validFormValues.newUserEmail },
  })
  fireEvent.change(screen.getByLabelText('Phone number'), {
    target: { value: validFormValues.contactNumber },
  })
}

function fillValidForm() {
  enterValuesIntoForm(validFormValues)
}

function submitForm() {
  fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))
}

function mockOnboardResponse(status: number) {
  vi.mocked(postUsersOnboard).mockResolvedValueOnce({
    data: undefined,
    error: {},
    response: new Response(null, { status }),
  })
}

describe('OrganisationOnboardUserForm', () => {
  it('renders the onboarding controls', () => {
    renderForm()

    expect(
      screen.getByText(
        'New users will be assigned the standard user role by default. You can change the permissions later using user management.',
      ),
    ).toBeDefined()
    expect(screen.getByLabelText('Full name')).toBeDefined()
    expect(screen.getByLabelText('Work email address')).toBeDefined()
    expect(screen.getByLabelText('Phone number')).toBeDefined()
    expect(screen.getByText('For international numbers include the country code.')).toBeDefined()
    expect(screen.getByRole('button', { name: 'Send invite' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Cancel' }).getAttribute('href')).toBe(
      '/portal/organisations/123',
    )
  })

  it('shows required validation errors when submitted empty', async () => {
    renderForm()

    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(await screen.findByText("Enter the user's full name")).toBeDefined()
    expect(await screen.findByText("Enter the user's work email address")).toBeDefined()
    expect(await screen.findByText("Enter the user's phone number")).toBeDefined()
    expect(postUsersOnboard).not.toHaveBeenCalled()
  })

  it('validates the phone number as a valid phone number', async () => {
    mocks.phoneNumberValidationMock.mockReturnValue(false)

    const examplePhoneNumber = '63846484638'
    renderForm()
    enterValuesIntoForm({ ...validFormValues, contactNumber: examplePhoneNumber })
    submitForm()

    await waitFor(async () => {
      expect(mocks.phoneNumberValidationMock).toHaveBeenCalledWith(examplePhoneNumber, 'GB')
      expect(await screen.findByText(errorMessages.phoneFormat)).toBeDefined()
    })
  })

  it('shows an email format validation error', async () => {
    renderForm()

    fireEvent.change(screen.getByLabelText('Full name'), {
      target: { value: 'Test User' },
    })
    fireEvent.change(screen.getByLabelText('Work email address'), {
      target: { value: 'not-an-email-address' },
    })
    fireEvent.change(screen.getByLabelText('Phone number'), {
      target: { value: '01234567890' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText(
        'Enter an email address in the correct format, like name@example.com',
      ),
    ).toBeDefined()
    expect(postUsersOnboard).not.toHaveBeenCalled()
  })

  it('revalidates fields on blur after a failed submit', async () => {
    renderForm()

    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(await screen.findByText("Enter the user's full name")).toBeDefined()
    expect(await screen.findByText("Enter the user's work email address")).toBeDefined()
    expect(await screen.findByText("Enter the user's phone number")).toBeDefined()

    fireEvent.change(screen.getByLabelText('Full name'), {
      target: { value: 'Test User' },
    })
    fireEvent.blur(screen.getByLabelText('Full name'))
    fireEvent.change(screen.getByLabelText('Work email address'), {
      target: { value: 'test@test.com' },
    })
    fireEvent.blur(screen.getByLabelText('Work email address'))
    fireEvent.change(screen.getByLabelText('Phone number'), {
      target: { value: '01234567890' },
    })
    fireEvent.blur(screen.getByLabelText('Phone number'))

    await waitFor(() => {
      expect(screen.queryByText("Enter the user's full name")).toBeNull()
      expect(screen.queryByText("Enter the user's work email address")).toBeNull()
      expect(screen.queryByText("Enter the user's phone number")).toBeNull()
    })
  })

  it('submits valid values and redirects to the organisation page with the invited email', async () => {
    vi.mocked(postUsersOnboard).mockResolvedValueOnce({
      data: undefined,
      error: undefined,
      response: new Response(null, { status: 201 }),
    })
    renderForm()

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    await waitFor(() => {
      expect(postUsersOnboard).toHaveBeenCalledWith({
        body: {
          fullName: 'Test User',
          newUserEmail: 'test@test.com',
          contactNumber: '01234567890',
          organisationId: 123,
        },
        credentials: 'include',
      })
      expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/123?invited=test%40test.com')
    })
  })

  it('shows a generic background error for an unhandled response status', async () => {
    mockOnboardResponse(500)
    renderForm()

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText('There was a problem sending the invite. Please try again later.'),
    ).toBeDefined()
  })

  it('shows a background error for invalid invite details', async () => {
    mockOnboardResponse(400)
    renderForm()

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText(
        'The invite details are invalid. Check the information and try again.',
      ),
    ).toBeDefined()
  })

  it('shows a background error when the user cannot invite users', async () => {
    mockOnboardResponse(403)
    renderForm()

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText('You do not have permission to invite users to this organisation.'),
    ).toBeDefined()
  })

  it('shows username conflicts as an email field error', async () => {
    mockOnboardResponse(409)
    renderForm()

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(await screen.findByText('A user with this email address already exists.')).toBeDefined()
    expect(screen.queryByRole('alert')).toBeNull()
  })

  it('clears the username conflict error when the email changes', async () => {
    mockOnboardResponse(409)
    renderForm()

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(await screen.findByText('A user with this email address already exists.')).toBeDefined()

    fireEvent.change(screen.getByLabelText('Work email address'), {
      target: { value: 'different@test.com' },
    })

    await waitFor(() => {
      expect(screen.queryByText('A user with this email address already exists.')).toBeNull()
    })
  })
})
