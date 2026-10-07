import { revalidatePath } from 'next/cache'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { updateOrganisationDetails } from '@/client/generated/sdk.gen'
import type { UpdateOrganisationDetailsDto } from '@/client/generated/types.gen'
import { createServerApiClient } from '@/client/server-api'

import { updateOrganisationDetailsAction } from './updateOrganisationDetails'

vi.mock('next/cache', () => ({ revalidatePath: vi.fn() }))
vi.mock('@/client/generated/sdk.gen', () => ({ updateOrganisationDetails: vi.fn() }))
vi.mock('@/client/server-api', () => ({ createServerApiClient: vi.fn() }))

const details = {
  organisationName: 'Acme Ltd',
  headOfficeAddress: '1 Example Street',
  headOfficeEmail: 'contact@example.com',
  headOfficeTelephone: '0121 234 5678',
} satisfies UpdateOrganisationDetailsDto

const client = {} as Awaited<ReturnType<typeof createServerApiClient>>

beforeEach(() => {
  vi.mocked(createServerApiClient).mockResolvedValue(client)
  vi.mocked(updateOrganisationDetails).mockResolvedValue({
    data: {
      ...details,
      id: 7,
      organisationType: 'PharmaCompany',
      allowedPharmaceuticalEntity: 'test-entity',
      status: 'Active',
    },
    error: undefined,
    response: new Response(null, { status: 200 }),
  })
})

describe('updateOrganisationDetailsAction', () => {
  it.each([0, -1, 1.5, NaN, Infinity, Number.MAX_SAFE_INTEGER + 1, '1', null, undefined])(
    'rejects invalid organisation ID %s before calling the backend',
    async (id) => {
      const result = await updateOrganisationDetailsAction(id as number, details)

      expect(result).toEqual({
        status: 'error',
        message: 'Check the organisation details and try again.',
      })
      expect(createServerApiClient).not.toHaveBeenCalled()
      expect(updateOrganisationDetails).not.toHaveBeenCalled()
      expect(revalidatePath).not.toHaveBeenCalled()
    },
  )

  it.each([
    null,
    undefined,
    {},
    { ...details, organisationName: ' ' },
    { ...details, headOfficeAddress: ' ' },
    { ...details, headOfficeEmail: 'not-an-email' },
    { ...details, headOfficeTelephone: 'not-a-phone' },
    { ...details, organisationName: 123 },
  ])('rejects malformed details %j before calling the backend', async (values) => {
    expect(
      await updateOrganisationDetailsAction(7, values as UpdateOrganisationDetailsDto),
    ).toEqual({ status: 'error', message: 'Check the organisation details and try again.' })
    expect(createServerApiClient).not.toHaveBeenCalled()
    expect(updateOrganisationDetails).not.toHaveBeenCalled()
    expect(revalidatePath).not.toHaveBeenCalled()
  })

  it('sends trimmed known fields with the authenticated client when input is valid', async () => {
    const result = await updateOrganisationDetailsAction(7, {
      ...Object.fromEntries(Object.entries(details).map(([key, value]) => [key, ` ${value} `])),
      privateField: 'must not be forwarded',
    } as unknown as UpdateOrganisationDetailsDto)

    expect(createServerApiClient).toHaveBeenCalledOnce()
    expect(updateOrganisationDetails).toHaveBeenCalledExactlyOnceWith({
      client,
      path: { id: 7 },
      body: details,
    })
    expect(result).toEqual({ status: 'success' })
    expect(vi.mocked(revalidatePath).mock.calls).toEqual([
      ['/portal/organisations/7'],
      ['/portal/organisations/7/edit'],
    ])
    expect(vi.mocked(updateOrganisationDetails).mock.invocationCallOrder[0]).toBeLessThan(
      vi.mocked(revalidatePath).mock.invocationCallOrder[0],
    )
  })

  it.each([400, 401, 403, 404, 500])(
    'returns a safe error without revalidation when the backend returns %i',
    async (status) => {
      vi.mocked(updateOrganisationDetails).mockResolvedValue({
        error: { status, detail: 'Private backend information' },
        data: undefined,
      })

      expect(await updateOrganisationDetailsAction(7, details)).toEqual({
        status: 'error',
        message: 'There was a problem updating the organisation. Please try again later.',
      })
      expect(revalidatePath).not.toHaveBeenCalled()
    },
  )

  it('propagates authentication control flow without mutation or revalidation', async () => {
    const redirect = new Error('NEXT_REDIRECT')
    vi.mocked(createServerApiClient).mockRejectedValueOnce(redirect)

    await expect(updateOrganisationDetailsAction(7, details)).rejects.toBe(redirect)
    expect(updateOrganisationDetails).not.toHaveBeenCalled()
    expect(revalidatePath).not.toHaveBeenCalled()
  })

  it('does not revalidate when the backend call throws', async () => {
    const error = new Error('Request failed')
    vi.mocked(updateOrganisationDetails).mockRejectedValueOnce(error)

    await expect(updateOrganisationDetailsAction(7, details)).rejects.toBe(error)
    expect(revalidatePath).not.toHaveBeenCalled()
  })
})
