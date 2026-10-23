import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecord, RecordDto } from '@/client/generated'
import {
  fakeRecordDtoMedicineRecordDto,
  fakeRecordProductDetailDto,
} from '@/client/generated/@faker-js/faker.gen'
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
  }: {
    organisationId: number
    recordId: number
  }) => (
    <div
      data-testid="discard-record-controls"
      data-organisation-id={organisationId}
      data-record-id={recordId}
    />
  ),
}))

type GetRecordResult = Awaited<ReturnType<typeof getRecord>>

const medicineRecord = {
  ...fakeRecordDtoMedicineRecordDto(),
  recordType: 'Medicine',
  recordStatus: 'Unpublished',
  displayStatus: 'Draft',
  recordProductDetail: fakeRecordProductDetailDto(),
} satisfies RecordDto

const organisationId = String(medicineRecord.organisationId)
const recordId = String(medicineRecord.recordId)

const mockRecord = (result: Partial<GetRecordResult>) =>
  vi.mocked(getRecord).mockResolvedValue(result as GetRecordResult)

const renderPage = async () => {
  const page = await DiscardRecordPage({
    params: Promise.resolve({ id: organisationId, recordId }),
    searchParams: Promise.resolve({ recordType: medicineRecord.recordType }),
  })
  // The organisation wrapper mock renders the async discard view directly, so resolve it here.
  const view = page.props.children({ id: medicineRecord.organisationId }) as ReactElement<unknown>
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
      expect.objectContaining({
        path: { id: medicineRecord.recordId },
        query: { recordType: medicineRecord.recordType },
      }),
    )
    expect(
      screen.getByRole('heading', { name: 'If you confirm, this record will be discarded' }),
    ).toBeDefined()
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
      'Confirm decision to discard record',
    )
    expect(screen.getByTestId('lead').textContent).toBe(
      `Record title: ${medicineRecord.recordProductDetail.recordTitle}`,
    )
    const controls = screen.getByTestId('discard-record-controls')
    expect(controls.getAttribute('data-organisation-id')).toBe(organisationId)
    expect(controls.getAttribute('data-record-id')).toBe(recordId)
  })

  it('links back to the record', async () => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      `/portal/organisations/${organisationId}/records/${recordId}?recordType=${medicineRecord.recordType}`,
    )
  })

  it.each([
    ['an invalid record ID', 'abc', medicineRecord.recordType],
    ['a missing record type', recordId, undefined],
    ['an invalid record type', recordId, 'Device'],
  ])('returns not found for %s', async (_, recordIdParam, recordType) => {
    await expect(
      DiscardRecordPage({
        params: Promise.resolve({ id: organisationId, recordId: recordIdParam }),
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
      data: { ...medicineRecord, organisationId: medicineRecord.organisationId + 1 },
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
