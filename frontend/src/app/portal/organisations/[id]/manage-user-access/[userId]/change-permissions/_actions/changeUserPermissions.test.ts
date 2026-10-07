import { revalidatePath } from 'next/cache'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { updateUserRole } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'

import { changeUserPermissionsAction } from './changeUserPermissions'

vi.mock('next/cache', () => ({ revalidatePath: vi.fn() }))
vi.mock('@/client/generated/sdk.gen', () => ({ updateUserRole: vi.fn() }))
vi.mock('@/client/server-api', () => ({ createServerApiClient: vi.fn() }))

const client = {} as Awaited<ReturnType<typeof createServerApiClient>>

beforeEach(() => {
  vi.mocked(createServerApiClient).mockResolvedValue(client)
  vi.mocked(updateUserRole).mockResolvedValue({
    data: {
      id: 9,
      userId: 4,
      organisationId: 7,
      userRole: 'Champion',
      status: 'Active',
      allowedPharmaceuticalEntity: 'test-entity',
      createdAt: '2026-01-01T00:00:00Z',
    },
    error: undefined,
    response: new Response(null, { status: 200 }),
  })
})

describe('changeUserPermissionsAction', () => {
  for (const index of [0, 1, 2]) {
    it.each([0, -1, 1.5, NaN, Infinity, Number.MAX_SAFE_INTEGER + 1, '1', null, undefined])(
      `rejects invalid ID %s at argument ${index} before calling the backend`,
      async (id) => {
        const ids = [7, 4, 9]
        ids[index] = id as number

        expect(await changeUserPermissionsAction(ids[0], ids[1], ids[2], 'Champion')).toEqual({
          status: 'error',
          message: "Check the user's permission details and try again.",
        })
        expect(createServerApiClient).not.toHaveBeenCalled()
        expect(updateUserRole).not.toHaveBeenCalled()
        expect(revalidatePath).not.toHaveBeenCalled()
      },
    )
  }

  it.each(['Super', 'Unknown', '', null, undefined, 1])(
    'rejects unsupported role %s before calling the backend',
    async (role) => {
      expect(await changeUserPermissionsAction(7, 4, 9, role as 'Standard' | 'Champion')).toEqual({
        status: 'error',
        message: "Check the user's permission details and try again.",
      })
      expect(createServerApiClient).not.toHaveBeenCalled()
      expect(updateUserRole).not.toHaveBeenCalled()
      expect(revalidatePath).not.toHaveBeenCalled()
    },
  )

  it.each(['Standard', 'Champion'] as const)(
    'sends role %s with the authenticated client and revalidates only after success',
    async (userRole) => {
      expect(await changeUserPermissionsAction(7, 4, 9, userRole)).toEqual({ status: 'success' })
      expect(createServerApiClient).toHaveBeenCalledOnce()
      expect(updateUserRole).toHaveBeenCalledExactlyOnceWith({
        client,
        path: { organisationId: 7, membershipId: 9 },
        body: { userRole },
      })
      expect(vi.mocked(revalidatePath).mock.calls).toEqual([
        ['/portal/organisations/7'],
        ['/portal/organisations/7/manage-user-access/4'],
        ['/portal/organisations/7/manage-user-access/4/change-permissions'],
      ])
      expect(vi.mocked(updateUserRole).mock.invocationCallOrder[0]).toBeLessThan(
        vi.mocked(revalidatePath).mock.invocationCallOrder[0],
      )
    },
  )

  it.each([400, 401, 403, 404, 500])(
    'returns a safe error without revalidation when the backend returns %i',
    async (status) => {
      vi.mocked(updateUserRole).mockResolvedValue({
        error: { status, detail: 'Private backend information' },
        data: undefined,
      })

      expect(await changeUserPermissionsAction(7, 4, 9, 'Champion')).toEqual({
        status: 'error',
        message: "There was a problem changing this user's permissions. Please try again later.",
      })
      expect(revalidatePath).not.toHaveBeenCalled()
    },
  )

  it('propagates authentication control flow without mutation or revalidation', async () => {
    const redirect = new Error('NEXT_REDIRECT')
    vi.mocked(createServerApiClient).mockRejectedValueOnce(redirect)

    await expect(changeUserPermissionsAction(7, 4, 9, 'Champion')).rejects.toBe(redirect)
    expect(updateUserRole).not.toHaveBeenCalled()
    expect(revalidatePath).not.toHaveBeenCalled()
  })

  it('does not revalidate when the backend call throws', async () => {
    const error = new Error('Request failed')
    vi.mocked(updateUserRole).mockRejectedValueOnce(error)

    await expect(changeUserPermissionsAction(7, 4, 9, 'Champion')).rejects.toBe(error)
    expect(revalidatePath).not.toHaveBeenCalled()
  })
})
