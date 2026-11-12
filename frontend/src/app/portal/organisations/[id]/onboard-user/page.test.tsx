import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getOrganisationById, getUsersMe } from '@/client/generated/sdk.gen'
import type { UserInformationDto, UserRole } from '@/client/generated/types.gen'
import { notFoundError, redirect } from '@/test-utils/nextNavigation'

import OrganisationOnboardUserPage from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUsersMe: vi.fn(),
  getOrganisationById: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('./_components/OrganisationOnboardUserForm', () => ({
  OrganisationOnboardUserForm: () => <form aria-label="Invite a user" />,
}))

const currentUser: UserInformationDto = {
  userId: 1,
  fullName: 'Julie Brooks',
  workTelephone: '01234 567890',
  workEmail: 'julie.brooks@example.com',
  organisationMembershipId: 9,
  organisationId: 2,
  organisationName: 'Example Pharma',
  userRole: 'Champion',
  status: 'Active',
  hasMultipleOrganisations: false,
  mfaMethod: 'Unknown',
  mfaPhoneNumber: '',
}

const params = Promise.resolve({ id: '2' })

beforeEach(() => {
  vi.mocked(getOrganisationById).mockResolvedValue({
    data: { id: 2, organisationName: 'Example Pharma' },
    error: undefined,
  } as never)
})

function mockCurrentUser(userRole: UserRole) {
  vi.mocked(getUsersMe).mockResolvedValue({
    data: { ...currentUser, userRole },
    error: undefined,
  } as never)
}

describe('OrganisationOnboardUserPage (direct invocation)', () => {
  it.each<UserRole>(['Champion', 'Super'])('shows the form to a %s user', async (userRole) => {
    mockCurrentUser(userRole)

    render(await OrganisationOnboardUserPage({ params }))

    expect(screen.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
    expect(screen.getByRole('form', { name: 'Invite a user' })).toBeInTheDocument()
  })

  it('tells a standard user they are not permitted, without the form', async () => {
    mockCurrentUser('Standard')

    render(await OrganisationOnboardUserPage({ params }))

    expect(
      screen.getByRole('heading', { name: 'User is not permitted to view this page' }),
    ).toBeVisible()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
    expect(getOrganisationById).not.toHaveBeenCalled()
  })

  it('lets a super user invite to another accessible organisation', async () => {
    mockCurrentUser('Super')

    render(await OrganisationOnboardUserPage({ params: Promise.resolve({ id: '3' }) }))

    expect(getOrganisationById).toHaveBeenCalledWith({ client: {}, path: { id: 3 } })
    expect(screen.getByRole('form', { name: 'Invite a user' })).toBeInTheDocument()
  })

  it.each([403, 404, 500])(
    'does not offer a form when the organisation returns %s',
    async (status) => {
      mockCurrentUser('Champion')
      vi.mocked(getOrganisationById).mockResolvedValue({
        data: undefined,
        error: { status },
      } as never)

      render(await OrganisationOnboardUserPage({ params: Promise.resolve({ id: '3' }) }))

      expect(screen.getByRole('heading', { name: 'Unable to load organisation' })).toBeVisible()
      expect(screen.queryByRole('form')).not.toBeInTheDocument()
    },
  )

  it('does not offer a form when organisation data is missing', async () => {
    mockCurrentUser('Champion')
    vi.mocked(getOrganisationById).mockResolvedValue({ data: undefined } as never)

    render(await OrganisationOnboardUserPage({ params }))

    expect(screen.getByRole('heading', { name: 'Unable to load organisation' })).toBeVisible()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
  })

  it('preserves the server client organisation-context redirect', async () => {
    mockCurrentUser('Champion')
    vi.mocked(getOrganisationById).mockImplementation(async () =>
      redirect('/portal/organisations/select'),
    )

    await expect(OrganisationOnboardUserPage({ params })).rejects.toThrow(
      'NEXT_REDIRECT: /portal/organisations/select',
    )
  })

  it.each(['0', '-1', '1.5', 'invalid'])(
    'rejects invalid organisation ID %s before fetching',
    async (id) => {
      await expect(OrganisationOnboardUserPage({ params: Promise.resolve({ id }) })).rejects.toBe(
        notFoundError,
      )
      expect(getUsersMe).not.toHaveBeenCalled()
      expect(getOrganisationById).not.toHaveBeenCalled()
    },
  )

  it('shows an error when the current user cannot be loaded', async () => {
    vi.mocked(getUsersMe).mockResolvedValue({ data: undefined, error: {} } as never)

    render(await OrganisationOnboardUserPage({ params }))

    expect(screen.getByRole('heading', { name: 'Unable to load current user' })).toBeVisible()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
  })
})
