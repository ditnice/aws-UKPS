import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getUsersMe } from '@/client/generated'
import { PharmaceuticalEntity, type UserRole } from '@/client/generated/types.gen'

import CreateMedicineRecordPage from './page'

vi.mock('@/client/generated', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@/client/generated')>()

  return {
    ...actual,
    getUsersMe: vi.fn(),
  }
})

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

vi.mock('../../_components/OrganisationPageWrapper', () => ({
  default: ({
    children,
  }: {
    children: (organisation: { id: number; organisationName: string }) => React.ReactNode
  }) =>
    children({
      id: 2,
      organisationName: 'Example Pharma',
    }),
}))

vi.mock('./CreateMedicineRecordForm', () => ({
  default: () => <form aria-label="Create record" />,
}))

vi.mock('./CreateRecordPageHeader', () => ({
  CreateRecordPageHeader: () => <h1>Create a record</h1>,
}))

const currentUser = {
  userId: 1,
  fullName: 'Julie Brooks',
  workTelephone: '01234 567890',
  workEmail: 'julie.brooks@example.com',
  organisationMembershipId: 9,
  organisationId: 2,
  organisationName: 'Example Pharma',
  userRole: 'Champion' as UserRole,
  status: 'Active',
  hasMultipleOrganisations: false,
  allowedPharmaceuticalEntity: PharmaceuticalEntity.BOTH,
}

function mockCurrentUser(userRole: UserRole) {
  vi.mocked(getUsersMe).mockResolvedValue({
    data: { ...currentUser, userRole },
    error: undefined,
  } as never)
}

const params = Promise.resolve({ id: '2' })

beforeEach(() => {
  vi.clearAllMocks()
})

describe('CreateMedicineRecordPage (direct invocation)', () => {
  it.each<UserRole>(['Standard', 'Champion'])('shows the form to a %s user', async (userRole) => {
    mockCurrentUser(userRole)

    render(await CreateMedicineRecordPage({ params }))

    expect(screen.getByRole('form', { name: 'Create record' })).toBeInTheDocument()
  })

  it('tells a super user they are not permitted, without the form', async () => {
    mockCurrentUser('Super')

    render(await CreateMedicineRecordPage({ params }))

    expect(
      screen.getByRole('heading', { name: 'User is not permitted to view this page' }),
    ).toBeVisible()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
  })
})
