import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecord, RecordDto } from '@/client/generated'
import {
  fakeRecordDtoMedicineRecordDto,
  fakeRecordProductDetailDto,
} from '@/client/generated/@faker-js/faker.gen'
import { createServerApiClient } from '@/client/server-api'

import ChangeRecordStatusPage from './page'

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
    preheading,
    breadcrumbs,
  }: {
    heading: string
    preheading: ReactNode
    breadcrumbs: ReactNode
  }) => (
    <>
      {breadcrumbs}
      <p data-testid="preheading">{preheading}</p>
      <h1>{heading}</h1>
    </>
  ),
}))

vi.mock('./_components/ChangeRecordStatusActions', () => ({
  ChangeRecordStatusActions: ({
    organisationId,
    recordId,
    recordStatus,
  }: {
    organisationId: number
    recordId: number
    recordStatus: string
  }) => (
    <div
      data-testid="change-record-status-actions"
      data-organisation-id={organisationId}
      data-record-id={recordId}
      data-record-status={recordStatus}
    />
  ),
}))

type GetRecordResult = Awaited<ReturnType<typeof getRecord>>

const medicineRecord = {
  ...fakeRecordDtoMedicineRecordDto(),
  recordType: 'Medicine',
  recordStatus: 'Published',
  displayStatus: 'Published',
  canDiscard: false,
  recordProductDetail: fakeRecordProductDetailDto(),
} satisfies RecordDto

const organisationId = String(medicineRecord.organisationId)
const recordId = String(medicineRecord.recordId)

const mockRecord = (result: Partial<GetRecordResult>) =>
  vi.mocked(getRecord).mockResolvedValue(result as GetRecordResult)

const renderPage = async () => {
  const page = await ChangeRecordStatusPage({
    params: Promise.resolve({ id: organisationId, recordId }),
    searchParams: Promise.resolve({ recordType: medicineRecord.recordType }),
  })
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

describe('ChangeRecordStatusPage', () => {
  it('renders the heading, record title and actions', async () => {
    mockRecord({ data: medicineRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(getRecord).toHaveBeenCalledWith(
      expect.objectContaining({
        path: { id: medicineRecord.recordId },
        query: { recordType: medicineRecord.recordType },
      }),
    )
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
      'What would you like to do with this record?',
    )
    expect(screen.getByTestId('preheading').textContent).toBe(
      medicineRecord.recordProductDetail.recordTitle,
    )
    const actions = screen.getByTestId('change-record-status-actions')
    expect(actions.getAttribute('data-organisation-id')).toBe(organisationId)
    expect(actions.getAttribute('data-record-id')).toBe(recordId)
    expect(actions.getAttribute('data-record-status')).toBe('Published')
  })

  it.each([
    ['Published', 'Published', 'If development has paused, you should place the record on hold.'],
    ['OnHold', 'OnHold', 'If development has restarted, you should remove the record hold.'],
  ] as const)('explains the options for a %s record', async (displayStatus, recordStatus, text) => {
    mockRecord({
      data: { ...medicineRecord, displayStatus, recordStatus },
      error: undefined,
      response: new Response(),
    })

    await renderPage()

    expect(screen.getByText(text)).toBeInTheDocument()
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
      ChangeRecordStatusPage({
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
    ['Archived', 'Archived'],
    ['QAReview', 'Unpublished'],
    ['Draft', 'Unpublished'],
  ] as const)(
    'returns not found for a %s record whose status cannot be changed',
    async (displayStatus, recordStatus) => {
      mockRecord({
        data: { ...medicineRecord, displayStatus, recordStatus },
        error: undefined,
        response: new Response(),
      })

      await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
    },
  )

  it('renders an error when the record cannot be retrieved', async () => {
    mockRecord({ data: undefined, error: {}, response: new Response(null, { status: 500 }) })

    await renderPage()

    expect(screen.getByRole('alert').textContent).toContain(
      'There was a problem retrieving the record',
    )
    expect(screen.queryByTestId('change-record-status-actions')).toBeNull()
  })
})
