import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getOrganisationById, getUsersMe } from '@/client/generated/sdk.gen'
import type { UserInformationDto, UserRole } from '@/client/generated/types.gen'

import EditOrganisationPage from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getOrganisationById: vi.fn(),
  getUsersMe: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('./_components/EditOrganisationDetailsForm', () => ({
  EditOrganisationDetailsForm: ({ organisationName }: { organisationName: string }) => (
    <form aria-label="Edit organisation details" data-organisation-name={organisationName} />
  ),
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

function mockCurrentUser(userRole: UserRole) {
  vi.mocked(getUsersMe).mockResolvedValue({
    data: { ...currentUser, userRole },
    error: undefined,
  } as never)
}

beforeEach(() => {
  vi.mocked(getOrganisationById).mockResolvedValue({
    data: {
      id: 2,
      organisationName: 'Example Pharma',
      headOfficeAddress: '1 High Street',
      headOfficeEmail: 'office@example.com',
      headOfficeTelephone: '01234 567890',
    },
    error: undefined,
  } as never)
})

describe('EditOrganisationPage (direct invocation)', () => {
  it.each<UserRole>(['Champion', 'Super'])('shows the form to a %s user', async (userRole) => {
    mockCurrentUser(userRole)

    render(await EditOrganisationPage({ params }))

    expect(screen.getByRole('heading', { name: "Edit your organisation's details" })).toBeVisible()
    expect(screen.getByRole('form', { name: 'Edit organisation details' })).toBeInTheDocument()
  })

  it('tells a standard user they are not permitted, without the form', async () => {
    mockCurrentUser('Standard')

    render(await EditOrganisationPage({ params }))

    expect(
      screen.getByRole('heading', { name: 'User is not permitted to view this page' }),
    ).toBeVisible()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
  })

  it('shows an error when the current user cannot be loaded', async () => {
    vi.mocked(getUsersMe).mockResolvedValue({ data: undefined, error: {} } as never)

    render(await EditOrganisationPage({ params }))

    expect(screen.getByRole('heading', { name: 'Unable to load current user' })).toBeVisible()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
  })
})
