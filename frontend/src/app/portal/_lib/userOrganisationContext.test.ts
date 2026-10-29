import { beforeEach, describe, expect, it, vi } from 'vitest'

import { unstable_rethrow } from '@/test-utils/nextNavigation'

import { getUserOrganisationContext } from './userOrganisationContext'

const mocks = vi.hoisted(() => ({
  createServerApiClient: vi.fn(),
  getUsersMe: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
vi.mock('server-only', () => ({}))
vi.mock('@/client/generated', () => ({ getUsersMe: mocks.getUsersMe }))
vi.mock('@/client/server-api', () => ({ createServerApiClient: mocks.createServerApiClient }))

const client = { id: 'server-client' }

beforeEach(() => {
  mocks.createServerApiClient.mockResolvedValue(client)
})

describe('getUserOrganisationContext', () => {
  it("returns the user's selected organisation context", async () => {
    mocks.getUsersMe.mockResolvedValue({
      data: {
        organisationId: 12,
        organisationName: 'Example Pharma',
        hasMultipleOrganisations: true,
      },
    })

    await expect(getUserOrganisationContext()).resolves.toEqual({
      organisationId: 12,
      organisationName: 'Example Pharma',
      hasMultipleOrganisations: true,
    })
  })

  it("requests the current user without the server client's redirecting fetch", async () => {
    mocks.getUsersMe.mockResolvedValue({ data: { organisationId: 12 } })

    await getUserOrganisationContext()

    expect(mocks.getUsersMe).toHaveBeenCalledWith({ client, fetch: globalThis.fetch })
  })

  it('returns undefined when the current user cannot be retrieved', async () => {
    mocks.getUsersMe.mockResolvedValue({ data: undefined, error: { status: 401 } })

    await expect(getUserOrganisationContext()).resolves.toBeUndefined()
  })

  it('returns undefined when the request fails', async () => {
    const error = new Error('Network error')
    mocks.getUsersMe.mockRejectedValue(error)

    await expect(getUserOrganisationContext()).resolves.toBeUndefined()
    expect(unstable_rethrow).toHaveBeenCalledWith(error)
  })
})
