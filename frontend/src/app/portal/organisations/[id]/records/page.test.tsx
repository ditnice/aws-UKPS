import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getCurrentOrganisationChampions, getUsersMe } from '@/client/generated'
import { PharmaceuticalEntity, type UserRole } from '@/client/generated/types.gen'

import { CreateRecordActionBanner } from './_components/CreateRecordActionBanner'
import OrganisationRecordsPage from './page'
import { RecordsFetch } from './RecordsFetch'

vi.mock('@/client/generated', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@/client/generated')>()

  return {
    ...actual,
    getUsersMe: vi.fn(),
    getCurrentOrganisationChampions: vi.fn(),
  }
})

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(() => Promise.resolve({})),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('../_components/OrganisationPageWrapper', () => ({
  default: ({
    children,
  }: {
    children: (organisation: { id: number; organisationName: string }) => React.ReactNode
  }) =>
    children({
      id: 2,
      organisationName: 'Example Pharma',
    }),
}))

vi.mock('@/app/portal/components/_components/TableAndFiltersGrid', () => ({
  TableAndFiltersGrid: ({
    title,
    filters,
    table,
  }: {
    title: string
    filters: React.ReactNode
    table: React.ReactNode
  }) => (
    <section>
      <h2>{title}</h2>
      {filters}
      {table}
    </section>
  ),
}))

vi.mock('./_components/CreateRecordActionBanner', () => ({
  CreateRecordActionBanner: vi.fn(() => <div data-testid="create-record-action-banner" />),
}))

vi.mock('./_components/RecordActionAlert', () => ({
  RecordActionAlert: ({ action }: { action: unknown }) => (
    <div role="alert">{JSON.stringify(action)}</div>
  ),
}))

vi.mock('./RecordsTablesFilters', () => ({
  default: () => <div data-testid="records-filters" />,
}))

vi.mock('./RecordsFetch', () => ({
  RecordsFetch: vi.fn(({ children }: { children: (data: unknown) => React.ReactNode }) =>
    children({ records: [] }),
  ),
}))

vi.mock('./RecordsQueryResultsSummary', () => ({
  RecordsQueryResultsSummary: () => <div data-testid="records-summary" />,
}))

vi.mock('./RecordsTable', () => ({
  default: () => <div data-testid="records-table" />,
}))

vi.mock('./recordsQuery', async (importOriginal) => {
  const actual = await importOriginal<typeof import('./recordsQuery')>()

  return {
    ...actual,
    parseQueryFromSearchParams: vi.fn(actual.parseQueryFromSearchParams),
  }
})

vi.mock('./_lib/recordActionAlert', async (importOriginal) => {
  const actual = await importOriginal<typeof import('./_lib/recordActionAlert')>()

  return {
    ...actual,
    parseRecordAction: vi.fn(actual.parseRecordAction),
  }
})

const currentUser = {
  userId: 1,
  fullName: 'Julie Brooks',
  workTelephone: '01234 567890',
  workEmail: 'julie.brooks@example.com',
  organisationMembershipId: 9,
  organisationId: 2,
  organisationName: 'Example Pharma',
  userRole: 'Champion' as UserRole,
  status: 'Active',
  hasMultipleOrganisations: false,
  allowedPharmaceuticalEntity: PharmaceuticalEntity.BOTH,
}

function mockCurrentUser(userRole: UserRole) {
  vi.mocked(getUsersMe).mockResolvedValue({
    data: { ...currentUser, userRole },
    error: undefined,
  } as never)
}

function mockChampionEmails(emails: string[] | undefined) {
  vi.mocked(getCurrentOrganisationChampions).mockResolvedValue({
    data: emails,
    error: undefined,
  } as never)
}

async function renderPage(
  options: {
    id?: string
    searchParams?: Record<string, string>
  } = {},
) {
  render(
    await OrganisationRecordsPage({
      params: Promise.resolve({ id: options.id ?? '2' }),
      searchParams: Promise.resolve(options.searchParams ?? {}),
    }),
  )
}

beforeEach(() => {
  vi.clearAllMocks()
  mockCurrentUser('Champion')
  mockChampionEmails([])
})

describe('OrganisationRecordsPage (direct invocation)', () => {
  it('displays the organisation name', async () => {
    await renderPage()

    expect(screen.getByRole('heading', { name: 'Example Pharma' })).toBeVisible()
  })

  it('displays a single champion email to a standard user', async () => {
    mockCurrentUser('Standard')
    mockChampionEmails(['champion@example.com'])

    await renderPage()

    expect(screen.getByText(/Your champion user is:/)).toBeVisible()
    expect(screen.getByRole('link', { name: 'champion@example.com' })).toHaveAttribute(
      'href',
      'mailto:champion@example.com',
    )
  })

  it('displays multiple champion emails to a standard user', async () => {
    mockCurrentUser('Standard')
    mockChampionEmails(['champion1@example.com', 'champion2@example.com'])

    await renderPage()

    expect(screen.getByText('Your champion users are:')).toBeVisible()

    expect(screen.getByRole('link', { name: 'champion1@example.com' })).toHaveAttribute(
      'href',
      'mailto:champion1@example.com',
    )

    expect(screen.getByRole('link', { name: 'champion2@example.com' })).toHaveAttribute(
      'href',
      'mailto:champion2@example.com',
    )

    expect(document.querySelectorAll('.multiple-champion-users li')).toHaveLength(2)
  })

  it.each<UserRole>(['Champion', 'Super'])(
    'does not display champion emails to a %s user',
    async (userRole) => {
      mockCurrentUser(userRole)
      mockChampionEmails(['champion@example.com'])

      await renderPage()

      expect(screen.queryByText(/Your champion user/)).not.toBeInTheDocument()

      expect(screen.queryByRole('link', { name: 'champion@example.com' })).not.toBeInTheDocument()
    },
  )

  it('does not display champion details when the email list is empty', async () => {
    mockCurrentUser('Standard')
    mockChampionEmails([])

    await renderPage()

    expect(screen.queryByText(/Your champion user/)).not.toBeInTheDocument()
  })

  it('does not display champion details when emails are undefined', async () => {
    mockCurrentUser('Standard')
    mockChampionEmails(undefined)

    await renderPage()

    expect(screen.queryByText(/Your champion user/)).not.toBeInTheDocument()
  })

  it('passes the organisation ID and pharmaceutical entity to the banner', async () => {
    await renderPage()

    expect(CreateRecordActionBanner).toHaveBeenCalledWith(
      expect.objectContaining({
        organisationId: '2',
        allowedPharmaceuticalEntity: PharmaceuticalEntity.BOTH,
      }),
      undefined,
    )

    expect(screen.getByTestId('create-record-action-banner')).toBeInTheDocument()
  })

  it('displays the records search and filter section', async () => {
    await renderPage()

    expect(screen.getByRole('heading', { name: 'Search and filter records' })).toBeVisible()

    expect(screen.getByTestId('records-filters')).toBeInTheDocument()
    expect(screen.getByTestId('records-summary')).toBeInTheDocument()
    expect(screen.getByTestId('records-table')).toBeInTheDocument()
  })

  it('passes the organisation ID to RecordsFetch', async () => {
    await renderPage()

    expect(RecordsFetch).toHaveBeenCalledWith(
      expect.objectContaining({
        organisationId: 2,
      }),
      undefined,
    )
  })

  it('shows the records table but hides user-dependent components when the current user cannot be loaded', async () => {
    vi.mocked(getUsersMe).mockResolvedValue({
      data: undefined,
      error: {},
    } as never)

    await renderPage()
    expect(getCurrentOrganisationChampions).not.toHaveBeenCalled()
    expect(screen.queryByTestId('create-record-action-banner')).not.toBeInTheDocument()
    expect(screen.getByTestId('records-table')).toBeInTheDocument()
  })
})
