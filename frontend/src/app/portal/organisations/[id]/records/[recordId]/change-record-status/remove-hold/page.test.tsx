import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecord, RecordDto } from '@/client/generated'
import {
  fakeRecordDtoMedicineRecordDto,
  fakeRecordProductDetailDto,
} from '@/client/generated/@faker-js/faker.gen'
import { createServerApiClient } from '@/client/server-api'

import RemoveHoldPage from './page'

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

vi.mock('../../../../_components/OrganisationPageWrapper', () => ({
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

vi.mock('./_components/RemoveHoldReasons', () => ({
  RemoveHoldReasons: ({
    organisationId,
    recordId,
    recordType,
  }: {
    organisationId: number
    recordId: number
    recordType: string
  }) => (
    <div
      data-testid="remove-hold-reasons"
      data-organisation-id={organisationId}
      data-record-id={recordId}
      data-record-type={recordType}
    />
  ),
}))

type GetRecordResult = Awaited<ReturnType<typeof getRecord>>

const onHoldRecord = {
  ...fakeRecordDtoMedicineRecordDto(),
  recordType: 'Medicine',
  recordStatus: 'OnHold',
  displayStatus: 'OnHold',
  actions: ['Update', 'ChangeStatus', 'ConfirmOnHold', 'ViewTimeline'],
  recordProductDetail: fakeRecordProductDetailDto(),
} satisfies RecordDto

const organisationId = String(onHoldRecord.organisationId)
const recordId = String(onHoldRecord.recordId)

const mockRecord = (result: Partial<GetRecordResult>) =>
  vi.mocked(getRecord).mockResolvedValue(result as GetRecordResult)

const renderPage = async (recordType: string = onHoldRecord.recordType) => {
  const page = await RemoveHoldPage({
    params: Promise.resolve({ id: organisationId, recordId }),
    searchParams: Promise.resolve({ recordType }),
  })
  const view = page.props.children({ id: onHoldRecord.organisationId }) as ReactElement<unknown>
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

describe('RemoveHoldPage', () => {
  it('renders the heading, record title, guidance and reasons', async () => {
    mockRecord({ data: onHoldRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(getRecord).toHaveBeenCalledWith(
      expect.objectContaining({
        path: { id: onHoldRecord.recordId },
        query: {
          recordType: onHoldRecord.recordType,
          organisationId: onHoldRecord.organisationId,
        },
      }),
    )
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe('Change record status')
    expect(screen.getByTestId('preheading').textContent).toBe(
      onHoldRecord.recordProductDetail.recordTitle,
    )
    expect(
      screen.getByText(
        'Removing a record from hold means development has restarted and the information in this record may now need updating.',
      ),
    ).toBeInTheDocument()
    const reasons = screen.getByTestId('remove-hold-reasons')
    expect(reasons.getAttribute('data-organisation-id')).toBe(organisationId)
    expect(reasons.getAttribute('data-record-id')).toBe(recordId)
    expect(reasons.getAttribute('data-record-type')).toBe('Medicine')
  })

  it('links back to the change record status page', async () => {
    mockRecord({ data: onHoldRecord, error: undefined, response: new Response() })

    await renderPage()

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
      `/portal/organisations/${organisationId}/records/${recordId}/change-record-status?recordType=${onHoldRecord.recordType}`,
    )
  })

  it.each([
    ['an invalid record ID', 'abc', onHoldRecord.recordType],
    ['a missing record type', recordId, undefined],
    ['an invalid record type', recordId, 'Device'],
  ])('returns not found for %s', async (_, recordIdParam, recordType) => {
    await expect(
      RemoveHoldPage({
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
      data: { ...onHoldRecord, organisationId: onHoldRecord.organisationId + 1 },
      error: undefined,
      response: new Response(),
    })

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('returns not found when the user cannot change the record status', async () => {
    mockRecord({
      data: { ...onHoldRecord, actions: ['ViewTimeline'] },
      error: undefined,
      response: new Response(),
    })

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('returns not found for a record that is not on hold', async () => {
    mockRecord({
      data: { ...onHoldRecord, recordStatus: 'Published', displayStatus: 'Published' },
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
    expect(screen.queryByTestId('remove-hold-reasons')).toBeNull()
  })
})
