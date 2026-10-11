import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { RecordListItemDto } from '@/client/generated'

import { parseQueryFromSearchParams } from './recordsQuery'
import RecordsTable from './RecordsTable'

vi.mock('next/navigation', () => ({ useRouter: () => ({ push: vi.fn() }) }))

afterEach(cleanup)

const record = (overrides: Partial<RecordListItemDto> = {}): RecordListItemDto => ({
  id: 5,
  recordType: 'Medicine',
  recordStatus: 'Unpublished',
  title: 'Chronic hepatitis C in adults',
  developmentName: 'GS-7977',
  reviewedAt: null,
  latestRevisionId: 7,
  firstPageId: 'indication',
  ...overrides,
})

const renderTable = async (items: RecordListItemDto[]) =>
  render(
    await RecordsTable({
      data: { items, totalCount: items.length, page: 1, pageSize: 20 },
      query: parseQueryFromSearchParams({}),
    }),
  )

describe('RecordsTable', () => {
  it("links to the first page of the latest revision's form", async () => {
    await renderTable([record()])

    const link = screen.getByRole('link', { name: 'View or edit Chronic hepatitis C in adults' })
    expect(link.getAttribute('href')).toBe('/portal/records/5/revisions/7/indication')
  })

  it('shows no action for a record type without a form', async () => {
    await renderTable([record({ recordType: 'Vaccine', firstPageId: null })])

    expect(screen.queryByRole('link', { name: /View or edit/ })).toBeNull()
  })
})
