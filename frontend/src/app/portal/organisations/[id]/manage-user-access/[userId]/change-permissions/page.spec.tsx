import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import type { UserInformationDto } from '@/client/generated/types.gen'

import ChangeUserPermissions from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUserDetailsWithinOrganisation: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

const notFound = vi.hoisted(() => vi.fn())
vi.mock('next/navigation', () => ({ notFound }))

vi.mock('./_components/ChangePermissionsForm', () => ({
  ChangePermissionsForm: ({
    currentRole,
    membershipId,
    organisationId,
    userId,
  }: {
    currentRole: string
    membershipId: number
    organisationId: number
    userId: number
  }) => (
    <div>
      Change permissions form: {currentRole} {membershipId} {organisationId} {userId}
    </div>
  ),
}))

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

function mockResponse(overrides: Partial<UserInformationDto> = {}, status = 200) {
  vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
    data: { ...user, ...overrides },
    response: { status } as Response,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
  } as any)
}

const params = Promise.resolve({ id: '2', userId: '4' })

beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(cleanup)

describe('ChangeUserPermissions', () => {
  it("requests the selected user's details within the organisation", async () => {
    mockResponse()

    render(await ChangeUserPermissions({ params }))

    expect(vi.mocked(getUserDetailsWithinOrganisation).mock.calls[0][0]).toMatchObject({
      path: { userId: 4, organisationId: 2 },
    })
  })

  it('explains what a standard user would gain', async () => {
    mockResponse({ userRole: 'Standard' })

    render(await ChangeUserPermissions({ params }))

    expect(screen.getByText('julie.brooks@example.com is a standard user.')).toBeDefined()
    expect(
      screen.getByText(
        'If you change this user’s role, they will gain access to additional capabilities in UK PharmaScan, including:',
      ),
    ).toBeDefined()
  })

  it('explains what a champion user would lose', async () => {
    mockResponse({ userRole: 'Champion' })

    render(await ChangeUserPermissions({ params }))

    expect(screen.getByText('julie.brooks@example.com is a champion user.')).toBeDefined()
    expect(
      screen.getByText(
        'If you change this user’s role, they will lose access to the following capabilities in UK PharmaScan:',
      ),
    ).toBeDefined()
  })

  it("passes the user's membership and role to the form", async () => {
    mockResponse({ userRole: 'Champion', organisationMembershipId: 9 })

    render(await ChangeUserPermissions({ params }))

    expect(screen.getByText('Change permissions form: Champion 9 2 4')).toBeDefined()
  })

  it('does not offer to change the role of a super user', async () => {
    mockResponse({ userRole: 'Super' })

    render(await ChangeUserPermissions({ params }))

    expect(screen.getByText('This user’s role cannot be changed from here.')).toBeDefined()
    expect(screen.queryByText(/Change permissions form/)).toBeNull()
  })

  it('calls notFound when the user is not a member of the organisation', async () => {
    vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
      response: { status: 404 } as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any)

    await ChangeUserPermissions({ params })

    expect(notFound).toHaveBeenCalled()
  })

  it('renders an error when the user cannot be retrieved', async () => {
    vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
      error: {},
      response: { status: 500 } as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any)

    render(await ChangeUserPermissions({ params }))

    expect(screen.getByRole('alert').textContent).toBe(
      'There was a problem retrieving the user. Please try again later.',
    )
  })
})
