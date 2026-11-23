import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getOrganisationRecords, getRecordDetail, RecordDetailDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import RecordPage from './page'

import type { ReactElement, ReactNode } from 'react'

const mocks = vi.hoisted(() => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('@/client/generated', async () => ({
  getRecordDetail: vi.fn(),
  getOrganisationRecords: vi.fn(),
  RecordType: (
    await vi.importActual<typeof import('@/client/generated/types.gen')>(
      '@/client/generated/types.gen',
    )
  ).RecordType,
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  notFound: mocks.notFound,
}))

vi.mock('../../_components/OrganisationPageWrapper', () => ({
  default: ({ children }: { children: (org: { id: number }) => ReactNode }) => children({ id: 7 }),
}))

vi.mock('@nice-digital/nds-page-header', () => ({
  PageHeader: ({
    heading,
    lead,
    metadata,
  }: {
    heading: string
    lead: ReactNode
    metadata: ReactNode[]
  }) => (
    <>
      <h1>{heading}</h1>
      <p data-testid="lead">{lead}</p>
      {metadata}
    </>
  ),
}))

vi.mock('./_components/ManageRecordActionBanner', () => ({
  ManageRecordActionBanner: () => <div data-testid="manage-record-banner" />,
}))

vi.mock('./_components/RecordViewAlert', async (importOriginal) => ({
  ...(await importOriginal<typeof import('./_components/RecordViewAlert')>()),
  RecordViewAlert: ({ action }: { action: string }) => (
    <div data-testid="record-alert" data-action={action} />
  ),
}))

vi.mock('./_components/RecordDetails', () => ({
  RecordDetails: ({ sections }: { sections: { id: string }[] }) => (
    <div data-testid="record-details" data-sections={sections.map((x) => x.id).join(',')} />
  ),
}))

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
  vi.mocked(getOrganisationRecords).mockResolvedValue({
    data: {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 10,
    },
    error: undefined,
    response: new Response(),
  })
})

afterEach(cleanup)

type GetRecordResult = Awaited<ReturnType<typeof getRecordDetail>>

const medicineRecord: RecordDetailDto = {
  overview: {
    recordType: 'Medicine',
    recordId: 42,
    organisationId: 7,
    recordStatus: 'Published',
    displayStatus: 'QAReview',
    reviewedAt: null,
    canEditContent: false,
    actions: ['ViewTimeline'],
    nextUpdateDue: null,
    companyCode: 'GC22',
    recordTitle: 'Early rheumatoid arthritis in adults',
  },
  sections: [
    { id: 'product-record-details', title: 'Product record details', optional: false, tasks: [] },
    { id: 'clinical-trials', title: 'Clinical trials', optional: false, tasks: [] },
  ],
}

const mockRecord = (result: Partial<GetRecordResult>) =>
  vi.mocked(getRecordDetail).mockResolvedValue(result as GetRecordResult)

const renderPage = async (
  recordId = '42',
  recordType: string | undefined = 'Medicine',
  action?: string,
) => {
  const page = await RecordPage({
    params: Promise.resolve({ id: '7', recordId }),
    searchParams: Promise.resolve({ recordType, action }),
  })
  // The organisation wrapper mock renders the async record view directly, so resolve it here.
  const view = page.props.children({ id: 7 }) as ReactElement<unknown>
  const viewComponent = view.type as (props: unknown) => Promise<ReactElement>
  render(await viewComponent(view.props))
}

describe('RecordPage', () => {
  it.each([
    { canEditContent: true, actions: ['ViewTimeline'], shown: true },
    { canEditContent: false, actions: ['ConfirmCurrent', 'ViewTimeline'], shown: true },
    { canEditContent: false, actions: ['ConfirmOnHold', 'ViewTimeline'], shown: true },
    { canEditContent: false, actions: ['ViewTimeline'], shown: false },
  ] as const)(
    'only says an overdue record can be edited or confirmed to a user who can ($canEditContent, $actions)',
    async ({ canEditContent, actions, shown }) => {
      mockRecord({
        data: {
          ...medicineRecord,
          overview: {
            ...medicineRecord.overview,
            canEditContent,
            actions: [...actions],
            nextUpdateDue: '2020-01-01T00:00:00Z',
          },
        },
        error: undefined,
        response: new Response(),
      })

      await renderPage()

      expect(screen.getByText(/This record needs to be reviewed for any updates\./)).toBeDefined()
      expect(screen.queryAllByText(/You can edit the record/)).toHaveLength(shown ? 1 : 0)
    },
  )

  it('renders the record header, manage banner and the sections in the response', async () => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(getRecordDetail).toHaveBeenCalledWith(
      expect.objectContaining({
        path: { id: 42 },
        query: { recordType: 'Medicine', organisationId: 7 },
      }),
    )
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe('Record 42: GC22')
    expect(screen.getByTestId('lead').textContent).toBe(
      'Record title: Early rheumatoid arthritis in adults',
    )
    expect(screen.getByText('Record title:').tagName).toBe('STRONG')
    expect(screen.getByText('QA review')).toBeDefined()
    expect(screen.getByTestId('manage-record-banner')).toBeDefined()
    expect(screen.getByTestId('record-details').getAttribute('data-sections')).toBe(
      'product-record-details,clinical-trials',
    )
  })

  it('says details are not yet available when the record has no sections', async () => {
    mockRecord({
      data: { overview: { ...medicineRecord.overview, recordType: 'Vaccine' }, sections: [] },
      error: undefined,
      response: new Response(),
    })

    await renderPage('42', 'Vaccine')

    expect(screen.queryByTestId('record-details')).toBeNull()
    expect(
      screen.getByText('Viewing the details of this type of record is not yet available.'),
    ).toBeDefined()
  })

  it('leaves the company code out of the heading when there is none', async () => {
    mockRecord({
      data: { ...medicineRecord, overview: { ...medicineRecord.overview, companyCode: null } },
      error: undefined,
      response: new Response(),
    })

    await renderPage()

    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe('Record 42')
  })

  it.each([
    ['an invalid organisation ID', 'abc', '42', 'Medicine'],
    ['an invalid record ID', '7', 'abc', 'Medicine'],
    ['a missing record type', '7', '42', undefined],
    ['an invalid record type', '7', '42', 'Device'],
  ])('returns not found for %s', async (_, id, recordId, recordType) => {
    await expect(
      RecordPage({
        params: Promise.resolve({ id, recordId }),
        searchParams: Promise.resolve({ recordType }),
      }),
    ).rejects.toThrow('NEXT_NOT_FOUND')
    expect(getRecordDetail).not.toHaveBeenCalled()
  })

  it('shows the alert for an action just taken', async () => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage('42', 'Medicine', 'archived')

    expect(screen.getByTestId('record-alert').getAttribute('data-action')).toBe('archived')
  })

  it.each([undefined, 'discarded'])('shows no alert for %s', async (action) => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage('42', 'Medicine', action)

    expect(screen.queryByTestId('record-alert')).toBeNull()
  })

  it('returns not found when the API cannot find the record', async () => {
    mockRecord({ data: undefined, error: {}, response: new Response(null, { status: 404 }) })

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('renders an error when the record cannot be retrieved', async () => {
    mockRecord({ data: undefined, error: {}, response: new Response(null, { status: 500 }) })

    await renderPage()

    expect(screen.getByRole('alert').textContent).toContain(
      'There was a problem retrieving the record',
    )
  })

  it.each([
    ['Published', 'View record details'],
    ['Archived', 'Archived record details'],
    ['OnHold', 'Published record details'],
    ['QAReview', 'Published record details'],
  ] as const)('displays "%s" record with heading "%s"', async (status, expectedHeading) => {
    mockRecord({
      data: {
        ...medicineRecord,
        overview: { ...medicineRecord.overview, displayStatus: status },
      },
      error: undefined,
      response: new Response(),
    })

    await renderPage()

    expect(
      screen.getByRole('heading', {
        level: 2,
        name: expectedHeading,
      }),
    ).toBeDefined()
  })
})
