import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { fakeUserInformationDto } from '@/client/generated/@faker-js/faker.gen'
import { getUserDetailsWithinOrganisation, getUsersMe } from '@/client/generated/sdk.gen'
import type { UserInformationDto } from '@/client/generated/types.gen'
import { notFound, notFoundError } from '@/test-utils/nextNavigation'

import ManageUserAccess from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUserDetailsWithinOrganisation: vi.fn(),
  getUsersMe: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
const user: UserInformationDto = {
  ...fakeUserInformationDto(),
  workEmail: 'julie.brooks@example.com',
  userRole: 'Standard',
  status: 'Active',
  hasMultipleOrganisations: false,
}
const currentUser: UserInformationDto = {
  ...fakeUserInformationDto(),
  userRole: 'Super',
}

function mockUserResponse(overrides: Partial<UserInformationDto> = {}) {
  vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
    data: { ...user, ...overrides },
    error: undefined,
  })
}

function mockCurrentUserResponse(overrides: Partial<UserInformationDto> = {}) {
  vi.mocked(getUsersMe).mockResolvedValue({
    data: { ...currentUser, ...overrides },
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
  mockUserResponse()
  mockCurrentUserResponse()
})
// Direct invocation checks orchestration and the returned synchronous tree only.
// It does not exercise Next.js rendering, routing, hydration or not-found handling.
describe('ManageUserAccess (direct invocation)', () => {
  it('returns the selected user summary and access actions', async () => {
    render(await ManageUserAccess({ params }))

    expect(screen.getByRole('heading', { name: "Manage user's access" })).toBeInTheDocument()
    expect(screen.getByText('julie.brooks@example.com is a standard user.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
      'href',
      '/portal/organisations/2',
    )
    expect(
      screen.getByRole('radio', {
        name: 'Change user permissions',
      }),
    ).toBeInTheDocument()
  })

  it('renders the remove user action when the current user is a Super user', async () => {
    mockCurrentUserResponse({ userRole: 'Super' })

    render(await ManageUserAccess({ params }))

    expect(
      screen.getByRole('radio', {
        name: 'Remove user',
      }),
    ).toBeInTheDocument()
  })

  it('uses the current user role rather than the selected user role when displaying the remove action', async () => {
    mockUserResponse({ userRole: 'Super' })
    mockCurrentUserResponse({ userRole: 'Champion' })

    render(await ManageUserAccess({ params }))

    expect(
      screen.queryByRole('radio', {
        name: 'Remove user',
      }),
    ).toBeNull()
  })

  it('calls notFound when the user is not a member of the organisation', async () => {
    mockErrorResponse(404)

    await expect(ManageUserAccess({ params })).rejects.toBe(notFoundError)

    expect(notFound).toHaveBeenCalledOnce()
  })

  it.each([
    ['Active', 3],
    ['Inactive', 3],
    ['AwaitingSetup', 1],
    ['Deactivated', 2],
  ] as const)('offers state-appropriate options for a %s user', async (status, actionCount) => {
    mockUserResponse({ status })
    render(await ManageUserAccess({ params }))
    expect(screen.getAllByRole('radio')).toHaveLength(actionCount)
    expect(screen.getByRole('radio', { name: 'Remove user' })).toBeInTheDocument()
  })

  it('rejects direct navigation to a removed user', async () => {
    mockUserResponse({ status: 'Removed' })
    await expect(ManageUserAccess({ params })).rejects.toBe(notFoundError)
  })

  it('does not offer self-management', async () => {
    mockCurrentUserResponse({ userId: user.userId })
    render(await ManageUserAccess({ params }))
    expect(screen.queryByRole('radio')).not.toBeInTheDocument()
    expect(
      screen.getByText('You cannot manage your own access from this page.'),
    ).toBeInTheDocument()
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
})
