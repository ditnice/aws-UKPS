import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import type { UserInformationDto } from '@/client/generated/types.gen'

import ManageUserAccess from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUserDetailsWithinOrganisation: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

const notFound = vi.hoisted(() => vi.fn())
vi.mock('next/navigation', () => ({ notFound }))

const user: UserInformationDto = {
  userId: 4,
  fullName: 'Julie Brooks',
  workTelephone: '01234 567890',
  workEmail: 'julie.brooks@example.com',
  organisationMembershipId: 9,
  organisationId: 2,
  organisationName: 'Example Pharma',
  userRole: 'Standard',
}

function mockResponse(overrides: Partial<UserInformationDto> = {}) {
  vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
    data: { ...user, ...overrides },
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

const params = Promise.resolve({ id: '2', userId: '4' })

beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(cleanup)

describe('ManageUserAccess', () => {
  it("requests the selected user's details within the organisation", async () => {
    mockResponse()

    render(await ManageUserAccess({ params }))

    expect(vi.mocked(getUserDetailsWithinOrganisation).mock.calls[0][0]).toMatchObject({
      path: { userId: 4, organisationId: 2 },
    })
  })

  it('describes a standard user by their role', async () => {
    mockResponse({ userRole: 'Standard' })

    render(await ManageUserAccess({ params }))

    expect(screen.getByText('julie.brooks@example.com is a standard user.')).toBeDefined()
  })

  it('describes a champion user by their role', async () => {
    mockResponse({ userRole: 'Champion' })

    render(await ManageUserAccess({ params }))

    expect(screen.getByText('julie.brooks@example.com is a champion user.')).toBeDefined()
  })

  it('links to the change permissions page', async () => {
    mockResponse()

    render(await ManageUserAccess({ params }))

    expect(screen.getByRole('link', { name: 'Change permissions' }).getAttribute('href')).toBe(
      '/portal/organisations/2/manage-user-access/4/change-permissions',
    )
  })

  it('does not offer to change permissions for a super user', async () => {
    mockResponse({ userRole: 'Super' })

    render(await ManageUserAccess({ params }))

    expect(screen.getByText('julie.brooks@example.com is a super user.')).toBeDefined()
    expect(screen.queryByRole('link', { name: 'Change permissions' })).toBeNull()
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
})
