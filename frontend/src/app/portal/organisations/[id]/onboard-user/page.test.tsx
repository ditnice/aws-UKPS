import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { getUsersMe } from '@/client/generated/sdk.gen'
import type { UserInformationDto, UserRole } from '@/client/generated/types.gen'

import OrganisationOnboardUserPage from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUsersMe: vi.fn(),
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
}

const params = Promise.resolve({ id: '2' })

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
  })

  it('shows an error when the current user cannot be loaded', async () => {
    vi.mocked(getUsersMe).mockResolvedValue({ data: undefined, error: {} } as never)

    render(await OrganisationOnboardUserPage({ params }))

    expect(screen.getByRole('heading', { name: 'Unable to load current user' })).toBeVisible()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
  })
})
