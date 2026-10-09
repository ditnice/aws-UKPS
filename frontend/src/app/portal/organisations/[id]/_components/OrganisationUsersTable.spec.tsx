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
  registrationRequestGuid: null,
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
  })

  it('uses the public request GUID for approval and rejection links', async () => {
    const requestGuid = 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d'
    vi.mocked(getUsers).mockResolvedValue({
      data: {
        items: [
          {
            userId: null,
            registrationRequestGuid: requestGuid,
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
      `/portal/organisations/2/registration-requests/${requestGuid}/approve`,
    )
    expect(screen.getByRole('link', { name: 'Reject' }).getAttribute('href')).toBe(
      `/portal/organisations/2/registration-requests/${requestGuid}/reject`,
    )
  })
})
