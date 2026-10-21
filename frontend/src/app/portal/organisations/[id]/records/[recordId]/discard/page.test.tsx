import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecord, RecordDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import DiscardRecordPage from './page'

import type { ReactElement, ReactNode } from 'react'

const mocks = vi.hoisted(() => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('@/client/generated', async () => ({
  getRecord: vi.fn(),
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

vi.mock('../../../_components/OrganisationPageWrapper', () => ({
  default: ({ children }: { children: (org: { id: number }) => ReactNode }) => children({ id: 7 }),
}))

vi.mock('@nice-digital/nds-page-header', () => ({
  PageHeader: ({
    heading,
    lead,
    breadcrumbs,
  }: {
    heading: string
    lead: ReactNode
    breadcrumbs: ReactNode
  }) => (
    <>
      {breadcrumbs}
      <h1>{heading}</h1>
      <p data-testid="lead">{lead}</p>
    </>
  ),
}))

vi.mock('./_components/DiscardRecordControls', () => ({
  DiscardRecordControls: ({
    organisationId,
    recordId,
    recordType,
  }: {
    organisationId: number
    recordId: number
    recordType: string
  }) => (
    <div
      data-testid="discard-record-controls"
      data-organisation-id={organisationId}
      data-record-id={recordId}
      data-record-type={recordType}
    />
  ),
}))

type GetRecordResult = Awaited<ReturnType<typeof getRecord>>

const medicineRecord: RecordDto = {
  recordType: 'Medicine',
  recordId: 42,
  organisationId: 7,
  recordStatus: 'Unpublished',
  displayStatus: 'Draft',
  revisionId: 3,
  recordClinicalTrials: [],
  recordProductDetail: {
    companyCode: 'GC22',
    recordTitle: 'Early rheumatoid arthritis in adults',
    namesAndIdentifiers: [],
  },
}

const mockRecord = (result: Partial<GetRecordResult>) =>
  vi.mocked(getRecord).mockResolvedValue(result as GetRecordResult)

const renderPage = async (recordId = '42', recordType: string | undefined = 'Medicine') => {
  const page = await DiscardRecordPage({
    params: Promise.resolve({ id: '7', recordId }),
    searchParams: Promise.resolve({ recordType }),
  })
  // The organisation wrapper mock renders the async discard view directly, so resolve it here.
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

describe('DiscardRecordPage', () => {
  it('renders the discard warning, record title and controls', async () => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(getRecord).toHaveBeenCalledWith(
      expect.objectContaining({ path: { id: 42 }, query: { recordType: 'Medicine' } }),
    )
    expect(
      screen.getByRole('heading', { name: 'If you confirm, this record will be discarded' }),
    ).toBeDefined()
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
      'Confirm decision to discard record',
    )
    expect(screen.getByTestId('lead').textContent).toBe(
      'Record title: Early rheumatoid arthritis in adults',
    )
    const controls = screen.getByTestId('discard-record-controls')
    expect(controls.getAttribute('data-organisation-id')).toBe('7')
    expect(controls.getAttribute('data-record-id')).toBe('42')
    expect(controls.getAttribute('data-record-type')).toBe('Medicine')
  })

  it('links back to the record', async () => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      '/portal/organisations/7/records/42?recordType=Medicine',
    )
  })

  it.each([
    ['an invalid record ID', 'abc', 'Medicine'],
    ['a missing record type', '42', undefined],
    ['an invalid record type', '42', 'Device'],
  ])('returns not found for %s', async (_, recordId, recordType) => {
    await expect(
      DiscardRecordPage({
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

  it.each([
    ['a draft of a published record', 'Draft', 'Published'],
    ['published', 'Published', 'Published'],
    ['in QA review', 'QAReview', 'Unpublished'],
    ['on hold', 'OnHold', 'OnHold'],
    ['archived', 'Archived', 'Archived'],
  ] as const)('returns not found when the record is %s', async (_, displayStatus, recordStatus) => {
    mockRecord({
      data: { ...medicineRecord, displayStatus, recordStatus },
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
    expect(screen.queryByTestId('discard-record-controls')).toBeNull()
  })
})
