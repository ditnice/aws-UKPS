import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getUserDetailsWithinOrganisation, getUsersMe } from '@/client/generated/sdk.gen'
import type { UserInformationDto, UserRole } from '@/client/generated/types.gen'

import ManageUserAccess from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUserDetailsWithinOrganisation: vi.fn(),
  getUsersMe: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
}))

const notFound = vi.hoisted(() => vi.fn())

vi.mock('next/navigation', () => ({
  notFound,
  useRouter: () => ({
    push: mocks.push,
  }),
}))
const userRole = 'Standard' as UserRole
const user: UserInformationDto = {
  userId: 4,
  fullName: 'Julie Brooks',
  workTelephone: '01234 567890',
  workEmail: 'julie.brooks@example.com',
  organisationMembershipId: 9,
  organisationId: 2,
  organisationName: 'Example Pharma',
  userRole: userRole,
}
const currentUserRole = 'Super' as UserRole
const currentUser = {
  userId: 10,
  fullName: 'Current User',
  workEmail: 'current.user@example.com',
  userRole: currentUserRole,
}

function mockUserResponse(overrides: Partial<UserInformationDto> = {}) {
  vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
    data: { ...user, ...overrides },
    error: undefined,
  })
}

function mockCurrentUserResponse(overrides = {}) {
  vi.mocked(getUsersMe).mockResolvedValue({
    data: {
      ...currentUser,
      ...overrides,
      workTelephone: '',
      organisationMembershipId: 1,
      organisationId: 1,
      organisationName: '',
    },
    error: undefined,
  })
}

function mockErrorResponse(status: number) {
  vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
    data: undefined,
    error: { status },
    response: new Response(null, { status }),
  })
}

const params = Promise.resolve({
  id: '2',
  userId: '4',
})

beforeEach(() => {
  vi.clearAllMocks()

  mockUserResponse()
  mockCurrentUserResponse()
})

afterEach(cleanup)

describe('ManageUserAccess', () => {
  it('renders the change user permissions action', async () => {
    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: 'Change user permissions',
      }),
    ).toBeDefined()
  })

  it('renders the deactivate user action', async () => {
    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: 'Deactivate user',
      }),
    ).toBeDefined()
  })

  it('renders the manage user details action', async () => {
    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: /Manage user details and sign in method/,
      }),
    ).toBeDefined()
  })

  it('renders the remove user action when the current user is a Super user', async () => {
    mockCurrentUserResponse({ userRole: 'Super' })

    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: 'Remove user - not implemented yet',
      }),
    ).toBeDefined()
  })

  it('does not render the remove user action when the current user is a Champion user', async () => {
    mockCurrentUserResponse({ userRole: 'Champion' })

    render(await ManageUserAccess({ params }))

    expect(
      screen.queryByRole('radio', {
        name: 'Remove user - not implemented yet',
      }),
    ).toBeNull()
  })

  it('uses the current user role rather than the selected user role when displaying the remove action', async () => {
    mockUserResponse({ userRole: 'Super' })
    mockCurrentUserResponse({ userRole: 'Champion' })

    render(await ManageUserAccess({ params }))

    expect(
      screen.queryByRole('radio', {
        name: 'Remove user - not implemented yet',
      }),
    ).toBeNull()
  })

  it('shows an error when Continue is clicked without selecting an action', async () => {
    render(await ManageUserAccess({ params }))

    fireEvent.click(
      screen.getByRole('button', {
        name: 'Continue',
      }),
    )

    expect(screen.getByText('Select an option - No answer provided')).toBeDefined()
  })

  it('calls notFound when the user is not a member of the organisation', async () => {
    mockErrorResponse(404)

    await ManageUserAccess({ params })

    expect(notFound).toHaveBeenCalled()
  })

  it('renders an error when the user cannot be retrieved', async () => {
    mockErrorResponse(500)

    render(await ManageUserAccess({ params }))

    expect(screen.getByRole('alert').textContent).toBe(
      'There was a problem retrieving the user. Please try again later.',
    )
  })

  it('navigates to change permissions when selected', async () => {
    render(await ManageUserAccess({ params }))
    fireEvent.click(
      screen.getByRole('radio', {
        name: 'Change user permissions',
      }),
    )
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Continue',
      }),
    )
    expect(mocks.push).toHaveBeenCalledExactlyOnceWith(
      '/portal/organisations/2/manage-user-access/4/change-permissions',
    )
  })

  it('navigates to deactivate user when selected', async () => {
    render(await ManageUserAccess({ params }))
    fireEvent.click(
      screen.getByRole('radio', {
        name: 'Deactivate user',
      }),
    )
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Continue',
      }),
    )
    expect(mocks.push).toHaveBeenCalledExactlyOnceWith('/portal/organisations/2/users/4/deactivate')
  })
  // add in tests for navigating to remove user and manage user access once it has been implemented
})
