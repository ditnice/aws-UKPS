import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getPublishedRecord, PublishedRecordDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import RecordPage from './page'

import type { ReactElement, ReactNode } from 'react'

const mocks = vi.hoisted(() => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('@/client/generated', () => ({
  getPublishedRecord: vi.fn(),
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

vi.mock('./_components/MedicineRecordDetails', () => ({
  MedicineRecordDetails: () => <div data-testid="medicine-record-details" />,
}))

type GetPublishedRecordResult = Awaited<ReturnType<typeof getPublishedRecord>>

const medicineRecord: PublishedRecordDto = {
  recordType: 'Medicine',
  recordId: 42,
  organisationId: 7,
  recordStatus: 'Active',
  revisionId: 3,
  recordClinicalTrials: [],
  recordProductDetail: {
    companyCode: 'GC22',
    recordTitle: 'Early rheumatoid arthritis in adults',
    namesAndIdentifiers: [],
  },
}

const mockRecord = (result: Partial<GetPublishedRecordResult>) =>
  vi.mocked(getPublishedRecord).mockResolvedValue(result as GetPublishedRecordResult)

const renderPage = async (recordId = '42', recordType: string | undefined = 'Medicine') => {
  const page = await RecordPage({
    params: Promise.resolve({ id: '7', recordId }),
    searchParams: Promise.resolve({ recordType }),
  })
  // The organisation wrapper mock renders the async record view directly, so resolve it here.
  const view = page.props.children({ id: 7 }) as ReactElement<unknown>
  const viewComponent = view.type as (props: unknown) => Promise<ReactElement>
  render(await viewComponent(view.props))
}

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
})

afterEach(cleanup)

describe('RecordPage', () => {
  it('renders the record header, manage banner and medicine details', async () => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(getPublishedRecord).toHaveBeenCalledWith(
      expect.objectContaining({ path: { id: 42 }, query: { recordType: 'Medicine' } }),
    )
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe('Record 42: GC22')
    expect(screen.getByTestId('lead').textContent).toBe(
      'Record title: Early rheumatoid arthritis in adults',
    )
    expect(screen.getByText('Record title:').tagName).toBe('STRONG')
    expect(screen.getByText('Active')).toBeDefined()
    expect(screen.getByTestId('manage-record-banner')).toBeDefined()
    expect(screen.getByTestId('medicine-record-details')).toBeDefined()
  })

  it.each([
    ['an invalid record ID', 'abc', 'Medicine'],
    ['a missing record type', '42', undefined],
    ['an invalid record type', '42', 'Device'],
  ])('returns not found for %s', async (_, recordId, recordType) => {
    await expect(
      RecordPage({
        params: Promise.resolve({ id: '7', recordId }),
        searchParams: Promise.resolve({ recordType }),
      }),
    ).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('returns not found when the API cannot find the record', async () => {
    mockRecord({ data: undefined, error: {}, response: new Response(null, { status: 404 }) })

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('returns not found when the record belongs to a different organisation', async () => {
    mockRecord({
      data: { ...medicineRecord, organisationId: 8 },
      error: undefined,
      response: new Response(),
    })

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('renders an error when the record cannot be retrieved', async () => {
    mockRecord({ data: undefined, error: {}, response: new Response(null, { status: 500 }) })

    await renderPage()

    expect(screen.getByRole('alert').textContent).toContain(
      'There was a problem retrieving the record',
    )
  })
})
