import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getOrganisationById } from '@/client/generated'
import type { OrganisationDetailsDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import OrganisationPageWrapper from './OrganisationPageWrapper'

vi.mock('@/client/generated', () => ({
  getOrganisationById: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

const mockedCreateServerApiClient = vi.mocked(createServerApiClient)
const mockedGetOrganisationById = vi.mocked(getOrganisationById)

// Direct invocation checks retrieval and the returned synchronous child tree only.
// It does not exercise Next.js async rendering or not-found handling.
describe('OrganisationPageWrapper (direct invocation)', () => {
  const organisation = {
    id: 123,
    organisationName: 'Example Pharma',
    organisationType: 'PharmaCompany',
    allowedPharmaceuticalEntity: 'Human',
    countryOrRegion: 'United Kingdom',
    headOfficeAddress: '1 Example Street, London',
    headOfficeEmail: 'office@example.com',
    headOfficeTelephone: '01234 567890',
    status: 'Active',
    lastActive: '2026-01-15T12:00:00Z',
    createdAt: '2025-06-01T12:00:00Z',
  } satisfies OrganisationDetailsDto

  beforeEach(() => {
    mockedCreateServerApiClient.mockResolvedValue(
      {} as Awaited<ReturnType<typeof createServerApiClient>>,
    )
  })

  it('renders the children with the retrieved organisation', async () => {
    mockedGetOrganisationById.mockResolvedValue({
      data: organisation,
      error: undefined,
    })

    const children = vi.fn(() => <div>Organisation content</div>)

    const result = await OrganisationPageWrapper({
      organisationId: '123',
      children,
    })

    render(result)

    expect(children).toHaveBeenCalledOnce()
    expect(children).toHaveBeenCalledWith(organisation)
    expect(screen.getByText('Organisation content')).toBeInTheDocument()
  })

  it('retrieves the organisation using the numeric organisation ID', async () => {
    mockedGetOrganisationById.mockResolvedValue({
      data: organisation,
      error: undefined,
    })

    const children = vi.fn(() => <div />)

    await OrganisationPageWrapper({
      organisationId: '123',
      children,
    })

    expect(mockedGetOrganisationById).toHaveBeenCalledOnce()
    expect(mockedGetOrganisationById).toHaveBeenCalledWith({
      client: expect.anything(),
      path: {
        id: 123,
      },
    })
  })

  it.each(['123.5', 'abc', '12abc', ''])(
    'calls notFound for an invalid organisation ID: %s',
    async (organisationId) => {
      const children = vi.fn(() => <div />)

      await expect(
        OrganisationPageWrapper({
          organisationId,
          children,
        }),
      ).rejects.toThrow('NEXT_NOT_FOUND')

      expect(mockedGetOrganisationById).not.toHaveBeenCalled()
      expect(children).not.toHaveBeenCalled()
    },
  )

  it('renders an error when retrieving the organisation fails', async () => {
    mockedGetOrganisationById.mockResolvedValue({
      data: undefined,
      error: { title: 'error' },
    })

    const children = vi.fn(() => <div>Organisation content</div>)

    const result = await OrganisationPageWrapper({
      organisationId: '123',
      children,
    })

    const { getByRole, getByTestId } = render(result)

    expect(getByRole('heading', { name: 'Unable to load organisation' })).toBeInTheDocument()
    expect(getByTestId('organisation-retrieval-error')).toBeInTheDocument()

    expect(children).not.toHaveBeenCalled()
  })
})
