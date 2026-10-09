import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { UserInformationDto, getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'

import EditDetails from './page'

vi.mock('@/client/generated', () => ({
  getUsersMe: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

// Isolate form prop wiring here; interaction and submission tests use the real form.
vi.mock('./_components/EditDetailsForm', () => ({
  EditDetailsForm: ({ userId, initialValues }: { userId: number; initialValues: unknown }) => (
    <div data-testid="edit-details-form">
      <span data-testid="user-id">{userId}</span>
      <span data-testid="initial-values">{JSON.stringify(initialValues)}</span>
    </div>
  ),
}))

const mockedGetUsersMe = vi.mocked(getUsersMe)
const apiClient = {} as Awaited<ReturnType<typeof createServerApiClient>>

beforeEach(() => {
  vi.mocked(createServerApiClient).mockResolvedValue(apiClient)
})

const exampleUser: UserInformationDto = {
  userId: 1,
  fullName: 'John Smith',
  workTelephone: '020 7123 4567',
  workEmail: 'john.smith@example.com',
  organisationMembershipId: 10,
  organisationId: 100,
  organisationName: 'Example Organisation',
  userRole: 'Standard',
}
// Direct invocation checks orchestration and the returned synchronous tree only.
// It does not exercise Next.js rendering, routing or hydration.
describe('EditDetails (direct invocation)', () => {
  it('renders the page header', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
      response: { ok: true } as Response,
    })

    const result = await EditDetails()

    render(result)

    expect(screen.getByRole('heading', { name: 'Edit your details' })).toBeInTheDocument()

    expect(screen.getByRole('link', { name: 'Back' })).toBeInTheDocument()
  })

  it('renders the edit details form when editing the current user', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    const result = await EditDetails()

    render(result)

    expect(getUsersMe).toHaveBeenCalledExactlyOnceWith({ client: apiClient })
    expect(screen.getByTestId('edit-details-form')).toBeInTheDocument()
    const textContent = screen.getByTestId('user-id').textContent.trim()
    expect(textContent).toBe(exampleUser.userId.toString())
    expect(screen.getByTestId('initial-values').textContent).toBe(
      JSON.stringify({
        fullName: exampleUser.fullName,
        workEmail: exampleUser.workEmail,
        workTelephone: exampleUser.workTelephone,
      }),
    )
  })

  it('renders an error when the current user cannot be retrieved', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: undefined,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    const result = await EditDetails()

    render(result)

    expect(screen.getByRole('alert')).toHaveTextContent(errorMessages.failedToRetrieveCurrentUser)

    expect(screen.queryByTestId('edit-details-form')).not.toBeInTheDocument()
  })
})
