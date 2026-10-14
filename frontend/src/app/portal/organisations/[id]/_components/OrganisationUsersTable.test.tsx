import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { Client } from '@/client/generated/client'
import { getUsers } from '@/client/generated/sdk.gen'
import type { UserListItemDto } from '@/client/generated/types.gen'

import { parseUserListQuery } from '../_lib/userListQuery'

import { OrganisationUsersTable } from './OrganisationUsersTable'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUsers: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}))

const apiClient = {} as Client

const query = {
  page: 1,
  pageSize: 10,
  status: [],
  role: [],
  email: undefined,
  lastActive: undefined,
  sortBy: undefined,
  sortDirection: undefined,
}

const removedUser: UserListItemDto = {
  userId: 5,
  registrationRequestId: null,
  emailAddress: 'removed-user-5@removed.invalid',
  role: 'Standard',
  status: 'Removed',
  lastActive: null,
  actions: [],
}

function mockUsersResponse(items: UserListItemDto[]) {
  vi.mocked(getUsers).mockResolvedValue({
    data: {
      items,
      totalCount: items.length,
      page: 1,
      pageSize: 10,
    },
    error: undefined,
  })
}

beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(cleanup)

describe('OrganisationUsersTable', () => {
  it.each(['AwaitingSetup', 'Active', 'Inactive', 'Deactivated'] as const)(
    'links a %s user to manage access when removal is permitted',
    async (status) => {
      mockUsersResponse([
        {
          ...removedUser,
          emailAddress: 'user@example.com',
          status,
          actions: ['RemoveUser'],
        },
      ])

      render(await OrganisationUsersTable({ apiClient, organisationId: 2, query }))

      expect(screen.getByRole('link', { name: 'Edit' })).toHaveAttribute(
        'href',
        '/portal/organisations/2/manage-user-access/5',
      )
      expect(screen.queryByRole('link', { name: 'Remove user' })).not.toBeInTheDocument()
    },
  )

  it('does not offer Edit for a user who can only be reactivated', async () => {
    mockUsersResponse([
      { ...removedUser, status: 'Deactivated', actions: ['ReactivateMembership'] },
    ])

    render(await OrganisationUsersTable({ apiClient, organisationId: 2, query }))

    expect(screen.queryByRole('link', { name: 'Remove user' })).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Edit' })).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Reactivate' })).toBeInTheDocument()
  })

  it('renders a single Edit link when several manage-access actions are permitted', async () => {
    mockUsersResponse([
      {
        ...removedUser,
        status: 'Active',
        actions: ['EditUserRole', 'DeactivateMembership', 'RemoveUser'],
      },
    ])

    render(await OrganisationUsersTable({ apiClient, organisationId: 2, query }))

    expect(screen.getByRole('link', { name: 'Edit' })).toHaveAttribute(
      'href',
      '/portal/organisations/2/manage-user-access/5',
    )
    expect(screen.getAllByRole('link', { name: 'Edit' })).toHaveLength(1)
    expect(screen.queryByRole('link', { name: 'Remove user' })).not.toBeInTheDocument()
  })

  it('renders Not applicable in the actions column for a removed user', async () => {
    mockUsersResponse([removedUser])

    render(
      await OrganisationUsersTable({
        apiClient,
        organisationId: 1,
        query,
      }),
    )

    const row = screen.getByText(removedUser.emailAddress).closest('tr')
    expect(row).not.toBeNull()
    expect(row?.textContent).toContain('Not applicable')
    expect(screen.queryByRole('link', { name: 'Remove user' })).not.toBeInTheDocument()
  })

  it('uses the registration request ID for approval and rejection links', async () => {
    const requestId = 42
    vi.mocked(getUsers).mockResolvedValue({
      data: {
        items: [
          {
            userId: null,
            registrationRequestId: requestId,
            emailAddress: 'test@example.com',
            role: 'Standard',
            status: 'RequestedAccess',
            lastActive: null,
            actions: ['ApproveMembership', 'RejectMembership'],
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 10,
      },
      error: undefined,
    })

    render(
      await OrganisationUsersTable({
        apiClient,
        organisationId: 2,
        query: parseUserListQuery({}),
      }),
    )

    expect(screen.getByRole('link', { name: 'Approve' }).getAttribute('href')).toBe(
      `/portal/organisations/2/registration-requests/${requestId}/approve`,
    )
    expect(screen.getByRole('link', { name: 'Reject' }).getAttribute('href')).toBe(
      `/portal/organisations/2/registration-requests/${requestId}/reject`,
    )
  })
})
