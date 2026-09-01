import { cleanup, render, screen } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'

import { CurrentUserInformationDto, getUsersMe } from '@/client/generated'
import { fakeCurrentUserInformationDto } from '@/client/generated/@faker-js/faker.gen'
import { errorMessages } from '@/lib/form/errorMessages'

import EditDetails from './page'

vi.mock('@/client/generated', () => ({
  getUsersMe: vi.fn(),
}))

vi.mock('@/components/BackLinkBrowser/BackLinkBrowser', () => ({
  BackLinkBrowser: () => <div data-testid="back-link" />,
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('@/components/PageHeader/PageHeader', () => ({
  PageHeader: ({ heading, backLink }: { heading: string; backLink: React.ReactNode }) => (
    <div data-testid="page-header">
      {backLink}
      <h1>{heading}</h1>
    </div>
  ),
}))

vi.mock('@/components/Placeholder/ErrorState', () => ({
  ErrorState: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="error-state">{children}</div>
  ),
}))

vi.mock('./_components/EditDetailsForm', () => ({
  EditDetailsForm: ({ userId, initialValues }: { userId: number; initialValues: unknown }) => (
    <div data-testid="edit-details-form">
      <span data-testid="user-id">{userId}</span>
      <span data-testid="initial-values">{JSON.stringify(initialValues)}</span>
    </div>
  ),
}))

const mockedGetUsersMe = vi.mocked(getUsersMe)

const exampleUser: CurrentUserInformationDto = {
  userId: 1,
  fullName: 'John Smith',
  workTelephone: '020 7123 4567',
  workEmail: 'john.smith@example.com',
  organisationMembershipId: 10,
  organisationId: 100,
  organisationName: 'Example Organisation',
  userRole: 'Standard',
}

beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(() => {
  cleanup()
})

describe('EditDetails', () => {
  it('renders the page header', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
      response: { ok: true } as Response,
    })

    const result = await EditDetails()

    render(result)

    expect(screen.getByRole('heading', { name: 'Edit your details' })).toBeDefined()

    expect(screen.getByTestId('back-link')).toBeDefined()
  })

  it('renders the edit details form when editing the current user', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    const result = await EditDetails()

    render(result)

    expect(screen.getByTestId('edit-details-form')).toBeDefined()
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

    expect(screen.getByTestId('error-state').textContent).toBe(
      errorMessages.failedToRetrieveCurrentUser,
    )

    expect(screen.queryByTestId('edit-details-form')).toBeFalsy()
  })

  it('throws when the current user id is not a number', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: { ...fakeCurrentUserInformationDto(), userId: 'incorrect-details' },
      response: { ok: true } as Response,
    })

    await expect(EditDetails()).rejects.toThrow(
      `Unexpected user details [UserId:incorrect-details].`,
    )
  })
})
