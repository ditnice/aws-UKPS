import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getUserDetailsWithinOrganisation, getUsersMe } from '@/client/generated/sdk.gen'
import type { UserInformationDto } from '@/client/generated/types.gen'

import RemoveUser from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUserDetailsWithinOrganisation: vi.fn(),
  getUsersMe: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

const notFound = vi.hoisted(() => vi.fn())
vi.mock('next/navigation', () => ({ notFound }))

vi.mock('./RemoveUserControls', () => ({
  default: ({ organisationId, userId }: { organisationId: number; userId: number }) => (
    <button
      data-testid="remove-user-controls"
      data-organisation-id={organisationId}
      data-user-id={userId}
    />
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
  status: 'Active',
}

function mockResponse() {
  vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
    data: user,
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
  notFound.mockImplementation(() => {
    throw new Error('NEXT_NOT_FOUND')
  })
  vi.mocked(getUsersMe).mockResolvedValue({
    data: { ...user, userId: 1, userRole: 'Super' },
    error: undefined,
  })
  mockResponse()
})

afterEach(cleanup)

describe('RemoveUser', () => {
  it("requests the selected user's details within the organisation", async () => {
    render(await RemoveUser({ params }))

    expect(vi.mocked(getUserDetailsWithinOrganisation).mock.calls[0][0]).toMatchObject({
      path: { userId: 4, organisationId: 2 },
    })
  })

  it('renders a back link to the selected user', async () => {
    render(await RemoveUser({ params }))

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      '/portal/organisations/2/manage-user-access/4',
    )
  })

  it('explains the removal and its effect on records', async () => {
    render(await RemoveUser({ params }))

    expect(
      screen.getByText(
        'You are about to permanently remove julie.brooks@example.com from UK PharmaScan and all organisations they belong to.',
      ),
    ).toBeDefined()
    expect(
      screen.getByText(
        'This will permanently remove their personal information from UK PharmaScan. Any records created by them will not be affected.',
      ),
    ).toBeDefined()
  })

  it('passes the organisation and user IDs to the controls', async () => {
    render(await RemoveUser({ params }))

    expect(screen.getByTestId('remove-user-controls').getAttribute('data-organisation-id')).toBe(
      '2',
    )
    expect(screen.getByTestId('remove-user-controls').getAttribute('data-user-id')).toBe('4')
  })

  it('calls notFound when the user is not a member of the organisation', async () => {
    mockErrorResponse(404)

    await expect(RemoveUser({ params })).rejects.toThrow('NEXT_NOT_FOUND')

    expect(notFound).toHaveBeenCalled()
  })

  it.each(['Standard', 'Champion'] as const)('rejects a %s caller', async (userRole) => {
    vi.mocked(getUsersMe).mockResolvedValue({ data: { ...user, userRole }, error: undefined })

    await expect(RemoveUser({ params })).rejects.toThrow('NEXT_NOT_FOUND')

    expect(getUserDetailsWithinOrganisation).not.toHaveBeenCalled()
  })

  it('calls notFound when the current user cannot be retrieved', async () => {
    vi.mocked(getUsersMe).mockResolvedValue({ data: undefined, error: { status: 500 } })

    await expect(RemoveUser({ params })).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('calls notFound for an already-removed user', async () => {
    vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
      data: { ...user, status: 'Removed' },
      error: undefined,
    })

    await expect(RemoveUser({ params })).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('uses the parsed user ID in the back link', async () => {
    render(await RemoveUser({ params: Promise.resolve({ id: '02', userId: '004' }) }))

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      '/portal/organisations/2/manage-user-access/4',
    )
  })

  it('renders an error when the user cannot be retrieved', async () => {
    mockErrorResponse(500)

    render(await RemoveUser({ params }))

    expect(screen.getByRole('alert').textContent).toBe(
      'There was a problem retrieving the user. Please try again later.',
    )
    expect(screen.queryByTestId('remove-user-controls')).toBeNull()
    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      '/portal/organisations/2/manage-user-access/4',
    )
  })
})
