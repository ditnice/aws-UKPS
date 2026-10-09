import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { Client } from '@/client/generated/client'
import { getUsers } from '@/client/generated/sdk.gen'

import { parseUserListQuery } from '../_lib/userListQuery'

import { OrganisationUsersTable } from './OrganisationUsersTable'

vi.mock('@/client/generated/sdk.gen', () => ({ getUsers: vi.fn() }))
vi.mock('next/navigation', () => ({ useRouter: () => ({ push: vi.fn() }) }))

afterEach(cleanup)

describe('OrganisationUsersTable', () => {
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
        apiClient: {} as Client,
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
