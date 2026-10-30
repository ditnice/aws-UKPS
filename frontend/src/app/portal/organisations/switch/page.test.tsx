import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getCurrentUserOrganisations, getUsersMe, UserInformationDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { notFound, redirect } from '@/test-utils/nextNavigation'

import SwitchOrganisationPage from './page'

vi.mock('@/client/generated', () => ({
  getCurrentUserOrganisations: vi.fn(),
  getUsersMe: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

// Isolate prop wiring here; switching behaviour uses the real controls in their tests.
vi.mock('./_components/SwitchOrganisationControls', () => ({
  SwitchOrganisationControls: (props: Record<string, unknown>) => (
    <div data-testid="switch-organisation-controls">{JSON.stringify(props)}</div>
  ),
}))

const currentUser: UserInformationDto = {
  userId: 1,
  fullName: 'John Smith',
  workTelephone: '020 7123 4567',
  workEmail: 'john.smith@example.com',
  organisationMembershipId: 10,
  organisationId: 1,
  organisationName: 'First organisation',
  userRole: 'Standard',
  status: 'Active',
  hasMultipleOrganisations: true,
}

const organisations = [
  { id: 1, organisationName: 'First organisation' },
  { id: 2, organisationName: 'Second organisation' },
]

type ApiResponses = {
  me?: UserInformationDto
  userOrganisations?: typeof organisations
}

// A key passed as undefined simulates a failed request.
const mockApi = (responses: ApiResponses = {}) => {
  const me = 'me' in responses ? responses.me : currentUser
  const userOrganisations =
    'userOrganisations' in responses ? responses.userOrganisations : organisations
  vi.mocked(getUsersMe).mockResolvedValue({ data: me } as Awaited<ReturnType<typeof getUsersMe>>)
  vi.mocked(getCurrentUserOrganisations).mockResolvedValue({
    data: userOrganisations,
  } as Awaited<ReturnType<typeof getCurrentUserOrganisations>>)
}

const renderPage = (searchParams: Record<string, string> = {}) =>
  SwitchOrganisationPage({ searchParams: Promise.resolve(searchParams) })

beforeEach(() => {
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
})

// Direct invocation checks orchestration and the returned synchronous tree only.
// It does not exercise Next.js rendering, redirects or browser-history behaviour.
describe('SwitchOrganisationPage (direct invocation)', () => {
  it('asks the user to confirm switching from their current organisation', async () => {
    mockApi()

    render(await renderPage({ organisationId: '2' }))

    expect(screen.getByRole('heading', { name: 'Switch organisation' })).toBeInTheDocument()
    expect(
      screen.getByText(
        'You are currently signed in for First organisation. Do you wish to switch to Second organisation?',
      ),
    ).toBeInTheDocument()
  })

  it.each([
    ['/portal/organisations/2/records/5', '/portal/organisations/2/records/5'],
    ['https://example.com', '/portal/organisations/2/records'],
  ])(
    "confirms to returnTo %s and cancels to the current organisation's records",
    async (returnTo, confirmHref) => {
      mockApi()

      render(await renderPage({ organisationId: '2', returnTo }))

      expect(JSON.parse(screen.getByTestId('switch-organisation-controls').textContent)).toEqual({
        organisationId: 2,
        confirmHref,
        cancelHref: '/portal/organisations/1/records',
      })
    },
  )

  it('redirects to returnTo when the organisation is already selected', async () => {
    mockApi({ me: { ...currentUser, organisationId: 2 } })

    await expect(
      renderPage({ organisationId: '2', returnTo: '/portal/organisations/2/records/5' }),
    ).rejects.toThrow('NEXT_REDIRECT')

    expect(redirect).toHaveBeenCalledWith('/portal/organisations/2/records/5', 'replace')
  })

  it('shows an error when the user cannot select the organisation', async () => {
    mockApi()

    render(await renderPage({ organisationId: '3' }))

    expect(screen.getByTestId('organisation-not-available')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Choose an organisation to manage' })).toHaveAttribute(
      'href',
      '/portal/organisations/select',
    )
    expect(screen.queryByTestId('switch-organisation-controls')).toBeNull()
  })

  it.each([
    ['the current user', { me: undefined }],
    ['the organisations', { userOrganisations: undefined }],
  ])('shows an error when %s cannot be retrieved', async (_, response) => {
    mockApi(response)

    render(await renderPage({ organisationId: '2' }))

    expect(screen.getByTestId('organisation-retrieval-error')).toBeInTheDocument()
    expect(screen.queryByTestId('switch-organisation-controls')).toBeNull()
  })

  it.each([undefined, '', 'abc', '0'])(
    'returns not found when the organisation id is %s',
    async (organisationId) => {
      await expect(
        renderPage(organisationId === undefined ? {} : { organisationId }),
      ).rejects.toThrow()

      expect(notFound).toHaveBeenCalled()
      expect(getUsersMe).not.toHaveBeenCalled()
    },
  )
})
