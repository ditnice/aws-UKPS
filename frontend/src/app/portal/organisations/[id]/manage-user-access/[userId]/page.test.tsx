import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

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
const notFoundError = new Error('Not found')

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
  notFound.mockImplementation(() => {
    throw notFoundError
  })

  mockUserResponse()
  mockCurrentUserResponse()
})
let interactions: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  interactions = userEvent.setup()
})

describe('ManageUserAccess', () => {
  it('renders the change user permissions action', async () => {
    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: 'Change user permissions',
      }),
    ).toBeInTheDocument()
  })

  it('renders the deactivate user action', async () => {
    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: 'Deactivate user',
      }),
    ).toBeInTheDocument()
  })

  it('renders the manage user details action', async () => {
    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: /Manage user details and sign in method/,
      }),
    ).toBeInTheDocument()
  })

  it('renders the remove user action when the current user is a Super user', async () => {
    mockCurrentUserResponse({ userRole: 'Super' })

    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: 'Remove user - not implemented yet',
      }),
    ).toBeInTheDocument()
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

    await interactions.click(
      screen.getByRole('button', {
        name: 'Continue',
      }),
    )

    expect(screen.getByText('Select an option - No answer provided')).toBeInTheDocument()
    expect(mocks.push).not.toHaveBeenCalled()
  })

  it('clears the selection error when an action is selected and navigates on Continue', async () => {
    render(await ManageUserAccess({ params }))
    await interactions.click(screen.getByRole('button', { name: 'Continue' }))
    expect(screen.getByText('Select an option - No answer provided')).toBeInTheDocument()

    await interactions.click(screen.getByRole('radio', { name: 'Change user permissions' }))
    expect(screen.queryByText('Select an option - No answer provided')).toBeNull()
    expect(mocks.push).not.toHaveBeenCalled()

    await interactions.click(screen.getByRole('button', { name: 'Continue' }))
    expect(mocks.push).toHaveBeenCalledExactlyOnceWith(
      '/portal/organisations/2/manage-user-access/4/change-permissions',
    )
  })

  it('calls notFound when the user is not a member of the organisation', async () => {
    mockErrorResponse(404)

    await expect(ManageUserAccess({ params })).rejects.toBe(notFoundError)

    expect(notFound).toHaveBeenCalledOnce()
  })

  it.each([
    { id: 'invalid', userId: '4' },
    { id: '1.5', userId: '4' },
    { id: '2', userId: 'invalid' },
    { id: '2', userId: '1.5' },
  ])('calls notFound for invalid route params %#', async (routeParams) => {
    await expect(ManageUserAccess({ params: Promise.resolve(routeParams) })).rejects.toBe(
      notFoundError,
    )

    expect(notFound).toHaveBeenCalledOnce()
    expect(getUsersMe).not.toHaveBeenCalled()
    expect(getUserDetailsWithinOrganisation).not.toHaveBeenCalled()
  })

  it('calls notFound when current-user data is missing', async () => {
    vi.mocked(getUsersMe).mockResolvedValue({ data: undefined, error: { status: 404 } })

    await expect(ManageUserAccess({ params })).rejects.toBe(notFoundError)

    expect(notFound).toHaveBeenCalledOnce()
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
    await interactions.click(
      screen.getByRole('radio', {
        name: 'Change user permissions',
      }),
    )
    await interactions.click(
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
    await interactions.click(
      screen.getByRole('radio', {
        name: 'Deactivate user',
      }),
    )
    await interactions.click(
      screen.getByRole('button', {
        name: 'Continue',
      }),
    )
    expect(mocks.push).toHaveBeenCalledExactlyOnceWith('/portal/organisations/2/users/4/deactivate')
  })
  // add in tests for navigating to remove user and manage user access once it has been implemented
})
