import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { postUsersOnboard } from '@/client/generated/sdk.gen'
import type { OnboardedUserDto } from '@/client/generated/types.gen'
import { NextLinkMock } from '@/test-utils/nextMocks'

import { OrganisationOnboardUserForm } from './OrganisationOnboardUserForm'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
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

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

function renderForm() {
  render(<OrganisationOnboardUserForm organisationId={123} />)
}

function fillValidForm() {
  fireEvent.change(screen.getByLabelText('Full name'), {
    target: { value: 'Test User' },
  })
  fireEvent.change(screen.getByLabelText('Work email address'), {
    target: { value: 'test@test.com' },
  })
  fireEvent.change(screen.getByLabelText('Phone number'), {
    target: { value: '01234567890' },
  })
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

  it('submits valid values and redirects to the organisation page with the new user id', async () => {
    mockSuccessfulOnboardResponse(456)
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
      expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/123?action=invited&userId=456')
    })
  })

  it('never puts the invited email address in the URL', async () => {
    mockSuccessfulOnboardResponse(456)
    renderForm()

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

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

    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: 'Send invite' }))

    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/123')
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
