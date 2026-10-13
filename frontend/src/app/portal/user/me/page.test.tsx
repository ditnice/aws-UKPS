import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { UserInformationDto } from '@/client/generated'
import { getUsersMe } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'

import Me from './page'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUsersMe: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

// Isolate page wiring here; the components have their own rendering tests.
vi.mock('./_components/UserAuthenticationDetails', () => ({
  UserAuthenticationDetails: ({ currentUser }: { currentUser: UserInformationDto }) => (
    <div data-testid="user-authentication-details">{currentUser.userId}</div>
  ),
}))

vi.mock('./_components/UserDetails', () => ({
  UserDetails: ({ currentUser }: { currentUser: UserInformationDto }) => (
    <div data-testid="user-details">{currentUser.userId}</div>
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

const renderPage = async (searchParams: { updated?: string } = {}) => {
  render(await Me({ searchParams: Promise.resolve(searchParams) }))
}

// Direct invocation checks orchestration and the returned synchronous tree only.
// It does not exercise Next.js rendering, routing or hydration.
describe('Me (direct invocation)', () => {
  it('renders the page header', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    await renderPage()

    expect(screen.getByRole('heading', { level: 1, name: 'Manage details' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back' })).toBeInTheDocument()
  })

  it("renders the current user's sign in method and personal details", async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    await renderPage()

    expect(getUsersMe).toHaveBeenCalledExactlyOnceWith({ client: apiClient })
    expect(screen.getByTestId('user-authentication-details')).toHaveTextContent(
      exampleUser.userId.toString(),
    )
    expect(screen.getByTestId('user-details')).toHaveTextContent(exampleUser.userId.toString())
  })

  it('renders an error when the current user cannot be retrieved', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: undefined,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    await renderPage()

    expect(screen.getByRole('alert')).toHaveTextContent(errorMessages.failedToRetrieveCurrentUser)
    expect(screen.queryByTestId('user-authentication-details')).not.toBeInTheDocument()
    expect(screen.queryByTestId('user-details')).not.toBeInTheDocument()
  })

  it('renders an error when the request returns an error', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
      error: { title: 'error' },
    } as unknown as Awaited<ReturnType<typeof getUsersMe>>)

    await renderPage()

    expect(screen.getByRole('alert')).toHaveTextContent(errorMessages.failedToRetrieveCurrentUser)
    expect(screen.queryByTestId('user-details')).not.toBeInTheDocument()
  })

  it.each(['true', 'TRUE'])('renders the success alert when updated is %s', async (updated) => {
    mockedGetUsersMe.mockResolvedValue({
      data: exampleUser,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    await renderPage({ updated })

    expect(
      screen.getByRole('heading', { level: 3, name: 'Details changed successfully' }),
    ).toBeInTheDocument()
    expect(
      screen.getByText('You can update your details at any time from manage my details.'),
    ).toBeInTheDocument()
  })

  it.each([undefined, 'false', 'yes'])(
    'does not render the success alert when updated is %s',
    async (updated) => {
      mockedGetUsersMe.mockResolvedValue({
        data: exampleUser,
      } as Awaited<ReturnType<typeof getUsersMe>>)

      await renderPage({ updated })

      expect(screen.queryByText('Details changed successfully')).not.toBeInTheDocument()
    },
  )
})
