import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { OnboardUserCommandDto } from '@/client/generated'
import { postUsersOnboard } from '@/client/generated/sdk.gen'
import type { OnboardedUserDto } from '@/client/generated/types.gen'
import { errorMessages } from '@/lib/form/errorMessages'
import { NextLinkMock } from '@/test-utils/nextMocks'
import { fillInput } from '@/test-utils/userInteractions'

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
type FormValues = Omit<OnboardUserCommandDto, 'organisationId'>
const validFormValues: FormValues = {
  fullName: 'Test User',
  newUserEmail: 'test@test.com',
  contactNumber: '01234567890',
}

function renderForm() {
  render(<OrganisationOnboardUserForm organisationId={123} />)
}

async function enterValuesIntoForm(validFormValues: FormValues) {
  await fillInput(user, screen.getByLabelText('Full name'), validFormValues.fullName)
  await fillInput(user, screen.getByLabelText('Work email address'), validFormValues.newUserEmail)
  await fillInput(user, screen.getByLabelText('Phone number'), validFormValues.contactNumber)
}

async function fillValidForm() {
  await enterValuesIntoForm(validFormValues)
}

async function submitForm() {
  await user.click(screen.getByRole('button', { name: 'Send invite' }))
}

function mockSuccessfulOnboardResponse(userId: number) {
  vi.mocked(postUsersOnboard).mockResolvedValueOnce({
    data: { userId },
    error: undefined,
    response: new Response(null, { status: 201 }),
  })
}

function mockOnboardResponse(status: number) {
  vi.mocked(postUsersOnboard).mockResolvedValueOnce({
    data: undefined,
    error: {},
    response: new Response(null, { status }),
  })
}

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('OrganisationOnboardUserForm', () => {
  it('renders the onboarding controls', () => {
    renderForm()

    expect(
      screen.getByText(
        'New users will be assigned the standard user role by default. You can change the permissions later using user management.',
      ),
    ).toBeInTheDocument()
    expect(screen.getByLabelText('Full name')).toBeInTheDocument()
    expect(screen.getByLabelText('Work email address')).toBeInTheDocument()
    expect(screen.getByLabelText('Phone number')).toBeInTheDocument()
    expect(
      screen.getByText('For international numbers include the country code.'),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Send invite' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Cancel' }).getAttribute('href')).toBe(
      '/portal/organisations/123',
    )
  })

  it('shows required validation errors when submitted empty', async () => {
    renderForm()

    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(await screen.findByText('Enter their full name')).toBeInTheDocument()
    expect(await screen.findByText('Enter an email address')).toBeInTheDocument()
    expect(await screen.findByText('Enter their phone number')).toBeInTheDocument()
    expect(postUsersOnboard).not.toHaveBeenCalled()
  })

  it('validates the phone number as a valid phone number', async () => {
    mocks.phoneNumberValidationMock.mockReturnValue(false)

    const examplePhoneNumber = '63846484638'
    renderForm()
    await enterValuesIntoForm({ ...validFormValues, contactNumber: examplePhoneNumber })
    await submitForm()

    await waitFor(async () => {
      expect(mocks.phoneNumberValidationMock).toHaveBeenCalledWith(examplePhoneNumber, 'GB')
      expect(await screen.findByText(errorMessages.phoneFormat)).toBeInTheDocument()
    })
  })

  it('shows an email format validation error', async () => {
    renderForm()

    await fillInput(user, screen.getByLabelText('Full name'), 'Test User')
    await fillInput(user, screen.getByLabelText('Work email address'), 'not-an-email-address')
    await fillInput(user, screen.getByLabelText('Phone number'), '01234567890')
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText(
        'Enter an email address in the correct format, like name@example.com',
      ),
    ).toBeInTheDocument()
    expect(postUsersOnboard).not.toHaveBeenCalled()
  })

  it('revalidates fields on blur after a failed submit', async () => {
    renderForm()

    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(await screen.findByText('Enter their full name')).toBeInTheDocument()
    expect(await screen.findByText('Enter an email address')).toBeInTheDocument()
    expect(await screen.findByText('Enter their phone number')).toBeInTheDocument()

    await fillInput(user, screen.getByLabelText('Full name'), 'Test User')
    await user.tab()
    await fillInput(user, screen.getByLabelText('Work email address'), 'test@test.com')
    await user.tab()
    await fillInput(user, screen.getByLabelText('Phone number'), '01234567890')
    await user.tab()

    await waitFor(() => {
      expect(screen.queryByText('Enter their full name')).toBeNull()
      expect(screen.queryByText('Enter an email address')).toBeNull()
      expect(screen.queryByText('Enter their phone number')).toBeNull()
    })
  })

  it('submits valid values and redirects to the organisation page with the new user id', async () => {
    mockSuccessfulOnboardResponse(456)
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

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
      expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/123?action=invited&userId=456')
    })
  })

  it('never puts the invited email address in the URL', async () => {
    mockSuccessfulOnboardResponse(456)
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    await waitFor(() => expect(mocks.push).toHaveBeenCalled())
    expect(mocks.push.mock.calls[0][0]).not.toContain('test')
  })

  it('redirects without an alert when the API does not return the new user id', async () => {
    vi.mocked(postUsersOnboard).mockResolvedValueOnce({
      // A success response whose body carries no id: the redirect should still
      // happen, just without the id the organisation page needs for the alert.
      data: {} as OnboardedUserDto,
      error: undefined,
      response: new Response(null, { status: 201 }),
    })
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/123')
    })
  })

  it('shows a generic background error for an unhandled response status', async () => {
    mockOnboardResponse(500)
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText('There was a problem sending the invite. Please try again later.'),
    ).toBeInTheDocument()
  })

  it('shows a background error for invalid invite details', async () => {
    mockOnboardResponse(400)
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText(
        'The invite details are invalid. Check the information and try again.',
      ),
    ).toBeInTheDocument()
  })

  it('shows a background error when the user cannot invite users', async () => {
    mockOnboardResponse(403)
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText('You do not have permission to invite users to this organisation.'),
    ).toBeInTheDocument()
  })

  it('shows username conflicts as an email field error', async () => {
    mockOnboardResponse(409)
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText('A user with this email address already exists.'),
    ).toBeInTheDocument()
    expect(screen.queryByRole('alert')).toBeNull()
  })

  it('clears the username conflict error when the email changes', async () => {
    mockOnboardResponse(409)
    renderForm()

    await fillValidForm()
    await user.click(screen.getByRole('button', { name: 'Send invite' }))

    expect(
      await screen.findByText('A user with this email address already exists.'),
    ).toBeInTheDocument()

    await fillInput(user, screen.getByLabelText('Work email address'), 'different@test.com')

    await waitFor(() => {
      expect(screen.queryByText('A user with this email address already exists.')).toBeNull()
    })
  })
})
