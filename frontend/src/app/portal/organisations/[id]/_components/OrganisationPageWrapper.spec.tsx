import { render } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getOrganisationById, OrganisationDetailsDto, ProblemDetails } from '@/client/generated'
import { fakeOrganisationDetailsDto } from '@/client/generated/@faker-js/faker.gen'
import { createServerApiClient } from '@/client/server-api'

import OrganisationPageWrapper from './OrganisationPageWrapper'

vi.mock('@/client/generated', () => ({
  getOrganisationById: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('@nice-digital/nds-page-header', () => ({
  PageHeader: ({ heading }: { heading: string }) => <h1>{heading}</h1>,
}))

const mockedCreateServerApiClient = vi.mocked(createServerApiClient)
const mockedGetOrganisationById = vi.mocked(getOrganisationById)

describe('OrganisationPageWrapper', () => {
  const organisation: OrganisationDetailsDto = fakeOrganisationDetailsDto()

  beforeEach(() => {
    vi.clearAllMocks()

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
    expect(result).toMatchInlineSnapshot(`
      <React.Fragment>
        <div>
          Organisation content
        </div>
      </React.Fragment>
    `)
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

    expect(getByRole('heading', { name: 'Unable to load organisation' })).toBeDefined()
    expect(getByTestId('organisation-retrieval-error')).toBeDefined()

    expect(children).not.toHaveBeenCalled()
  })
})
