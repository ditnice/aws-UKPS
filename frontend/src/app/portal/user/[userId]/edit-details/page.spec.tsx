import { describe, expect, it, vi, beforeEach } from 'vitest'

import { getUsersMe } from '@/client/generated'
import { fakeCurrentUserInformationDto } from '@/client/generated/@faker-js/faker.gen'
import { errorMessages } from '@/lib/form/errorMessages'

import EditDetails from './page'

vi.mock('@/client/generated', () => ({
  getUsersMe: vi.fn(),
}))

vi.mock('@/components/BackLinkBrowser/BackLinkBrowser', () => ({
  BackLinkBrowser: () => <div data-testid="back-link" />,
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

describe('EditDetails', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the page header', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: {
        userId: 123,
        fullName: 'John Smith',
        workEmail: 'john@example.com',
        workTelephone: '01234567890',
      },
    } as Awaited<ReturnType<typeof getUsersMe>>)

    const result = await EditDetails({
      params: Promise.resolve({ userId: 'me' }),
    })

    expect(result).toBeDefined()

    // The returned fragment contains the PageHeader and page content.
    expect(result.props.children).toHaveLength(2)
  })

  it('renders the edit details form when editing the current user', async () => {
    const currentUser = {
      userId: 123,
      fullName: 'John Smith',
      workEmail: 'john@example.com',
      workTelephone: '01234567890',
    }

    mockedGetUsersMe.mockResolvedValue({
      data: currentUser,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    const result = await EditDetails({
      params: Promise.resolve({ userId: 'me' }),
    })

    const content = result.props.children[1]

    expect(content.type).toBeDefined()
    expect(content.props.userId).toBe(123)
    expect(content.props.initialValues).toEqual(currentUser)
  })

  it('renders an error when attempting to edit another user', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: fakeCurrentUserInformationDto(),
      response: { ok: true } as Response,
    })

    const result = await EditDetails({
      params: Promise.resolve({ userId: '456' }),
    })

    const content = result.props.children[1]

    expect(content.props.children).toBe(errorMessages.editingAnotherUserIsNotCurrentSupported)
  })

  it('renders an error when the current user cannot be retrieved', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: undefined,
    } as Awaited<ReturnType<typeof getUsersMe>>)

    const result = await EditDetails({
      params: Promise.resolve({ userId: 'me' }),
    })

    const content = result.props.children[1]

    expect(content.props.children).toBe(errorMessages.failedToRetrieveCurrentUser)
  })

  it('throws when the current user id is not a number', async () => {
    mockedGetUsersMe.mockResolvedValue({
      data: fakeCurrentUserInformationDto(),
      response: { ok: true } as Response,
    })

    await expect(
      EditDetails({
        params: Promise.resolve({ userId: 'me' }),
      }),
    ).rejects.toThrow('Unexpected user details.')
  })
})
