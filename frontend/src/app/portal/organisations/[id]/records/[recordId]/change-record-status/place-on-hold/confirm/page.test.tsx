import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecord, RecordDto } from '@/client/generated'
import {
  fakeRecordDtoMedicineRecordDto,
  fakeRecordProductDetailDto,
} from '@/client/generated/@faker-js/faker.gen'
import { createServerApiClient } from '@/client/server-api'

import PlaceOnHoldConfirmPage from './page'

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

vi.mock('../../../../../_components/OrganisationPageWrapper', () => ({
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

vi.mock('../../_components/ConfirmStatusChangeControls', () => ({
  ConfirmStatusChangeControls: ({
    organisationId,
    recordId,
    recordType,
    action,
  }: {
    organisationId: number
    recordId: number
    recordType: string
    action: string
  }) => (
    <div
      data-testid="confirm-status-change"
      data-organisation-id={organisationId}
      data-record-id={recordId}
      data-record-type={recordType}
      data-action={action}
    />
  ),
}))

type GetRecordResult = Awaited<ReturnType<typeof getRecord>>

const publishedRecord = {
  ...fakeRecordDtoMedicineRecordDto(),
  recordType: 'Medicine',
  recordStatus: 'Published',
  displayStatus: 'Published',
  actions: ['Update', 'ChangeStatus', 'ConfirmCurrent', 'ViewTimeline'],
  recordProductDetail: fakeRecordProductDetailDto(),
} satisfies RecordDto

const organisationId = String(publishedRecord.organisationId)
const recordId = String(publishedRecord.recordId)

const mockRecord = (result: Partial<GetRecordResult>) =>
  vi.mocked(getRecord).mockResolvedValue(result as GetRecordResult)

const renderPage = async (recordType: string = publishedRecord.recordType) => {
  const page = await PlaceOnHoldConfirmPage({
    params: Promise.resolve({ id: organisationId, recordId }),
    searchParams: Promise.resolve({ recordType }),
  })
  const view = page.props.children({ id: publishedRecord.organisationId }) as ReactElement<unknown>
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

describe('PlaceOnHoldConfirmPage', () => {
  it('renders the warning, heading, record title, text and confirmation', async () => {
    mockRecord({ data: publishedRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(getRecord).toHaveBeenCalledWith(
      expect.objectContaining({
        path: { id: publishedRecord.recordId },
        query: {
          recordType: publishedRecord.recordType,
          organisationId: publishedRecord.organisationId,
        },
      }),
    )
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
      'Confirm decision to place the record on hold',
    )
    expect(screen.getByTestId('preheading').textContent).toBe(
      publishedRecord.recordProductDetail.recordTitle,
    )
    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'If you continue, this record will be placed on hold',
      }),
    ).toBeInTheDocument()
    expect(screen.getByText('What happens when this product is on hold:')).toBeInTheDocument()
    expect(screen.getAllByRole('listitem')).toHaveLength(3)
    const confirm = screen.getByTestId('confirm-status-change')
    expect(confirm.getAttribute('data-organisation-id')).toBe(organisationId)
    expect(confirm.getAttribute('data-record-id')).toBe(recordId)
    expect(confirm.getAttribute('data-record-type')).toBe('Medicine')
    expect(confirm.getAttribute('data-action')).toBe('PlaceOnHold')
  })

  it('links back to the reason page', async () => {
    mockRecord({ data: publishedRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      `/portal/organisations/${organisationId}/records/${recordId}/change-record-status/place-on-hold?recordType=${publishedRecord.recordType}`,
    )
  })

  it.each([
    ['an invalid record ID', 'abc', publishedRecord.recordType],
    ['a missing record type', recordId, undefined],
    ['an invalid record type', recordId, 'Device'],
  ])('returns not found for %s', async (_, recordIdParam, recordType) => {
    await expect(
      PlaceOnHoldConfirmPage({
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
      data: { ...publishedRecord, organisationId: publishedRecord.organisationId + 1 },
      error: undefined,
      response: new Response(),
    })

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('returns not found when the user cannot change the record status', async () => {
    mockRecord({
      data: { ...publishedRecord, actions: ['ViewTimeline'] },
      error: undefined,
      response: new Response(),
    })

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it.each(['OnHold', 'Archived', 'Unpublished'] as const)(
    'returns not found for a record that is %s',
    async (recordStatus) => {
      mockRecord({
        data: { ...publishedRecord, recordStatus },
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
    expect(screen.queryByTestId('confirm-status-change')).toBeNull()
  })
})
