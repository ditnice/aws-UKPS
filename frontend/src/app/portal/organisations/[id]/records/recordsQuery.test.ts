import { describe, expect, it } from 'vitest'

import {
  buildQueryFromFilters,
  convertQueryToSearchParams,
  getActiveFilters,
  parseQueryFromSearchParams,
} from './recordsQuery'

import type { RecordsQuery } from './recordsQuery'

const query: RecordsQuery = {
  search: 'medicine & vaccine',
  recordType: ['Medicine', 'Vaccine'],
  recordStatus: ['Active', 'OnHold'],
  updateStatus: 'NotOverdue',
  page: 3,
  pageSize: 25,
  sortBy: 'DevelopmentName',
  sortDirection: 'Descending',
}

describe('records query parsing', () => {
  it('uses pagination defaults and empty filters when search params are absent', () => {
    expect(parseQueryFromSearchParams({})).toEqual({
      search: undefined,
      recordType: [],
      recordStatus: [],
      updateStatus: undefined,
      page: 1,
      pageSize: 10,
      sortBy: undefined,
      sortDirection: undefined,
    })
  })

  it('parses repeated filters, pagination and sorting without changing the input', () => {
    const params = {
      search: query.search,
      recordType: ['Medicine', 'Vaccine'],
      recordStatus: ['Active', 'OnHold'],
      updateStatus: 'NotOverdue',
      page: '3',
      pageSize: '25',
      sortBy: 'DevelopmentName',
      sortDirection: 'Descending',
    }
    const original = structuredClone(params)

    expect(parseQueryFromSearchParams(params)).toEqual(query)
    expect(params).toEqual(original)
  })

  it('accepts scalar multi-value filters', () => {
    expect(
      parseQueryFromSearchParams({ recordType: 'Vaccine', recordStatus: 'Archived' }),
    ).toMatchObject({ recordType: ['Vaccine'], recordStatus: ['Archived'] })
  })

  it('discards unknown and incorrectly cased filters and sort values', () => {
    expect(
      parseQueryFromSearchParams({
        recordType: ['unknown', 'Medicine', 'vaccine'],
        recordStatus: ['active', 'OnHold', 'unknown'],
        updateStatus: 'overdue',
        sortBy: 'unknown',
        sortDirection: 'ascending',
        page: '-1',
        pageSize: '20',
      }),
    ).toMatchObject({
      recordType: ['Medicine'],
      recordStatus: ['OnHold'],
      updateStatus: undefined,
      sortBy: undefined,
      sortDirection: undefined,
      page: 1,
      pageSize: 10,
    })
  })

  it.each(['Id', 'DevelopmentName', 'RecordStatus', 'NextUpdateDue'])(
    'accepts the %s sort column',
    (sortBy) => {
      expect(parseQueryFromSearchParams({ sortBy }).sortBy).toBe(sortBy)
    },
  )

  it.each(['Overdue', 'NotOverdue'])('accepts the %s update status', (updateStatus) => {
    expect(parseQueryFromSearchParams({ updateStatus }).updateStatus).toBe(updateStatus)
  })
})

describe('active records filters', () => {
  it('returns no active filters for absent or empty values', () => {
    expect(getActiveFilters({})).toEqual([])
    expect(getActiveFilters({ search: '', recordStatus: [] })).toEqual([])
  })

  it('labels search, record statuses and update status in display order', () => {
    expect(getActiveFilters(query)).toEqual([
      { key: 'search', value: 'medicine & vaccine', label: 'medicine & vaccine' },
      { key: 'record-status', value: 'Active', label: 'Active' },
      { key: 'record-status', value: 'OnHold', label: 'On Hold' },
      { key: 'update-status', value: 'NotOverdue', label: 'Not overdue' },
    ])
  })

  it('rebuilds remaining filters while preserving type, sorting and pagination', () => {
    const original = structuredClone(query)
    const remaining = getActiveFilters(query).filter((filter) => filter.value !== 'Active')

    expect(buildQueryFromFilters(remaining, query)).toEqual({ ...query, recordStatus: ['OnHold'] })
    expect(query).toEqual(original)
  })

  it('clears all displayed filters when none remain', () => {
    expect(buildQueryFromFilters([], query)).toEqual({
      ...query,
      search: undefined,
      recordStatus: [],
      updateStatus: undefined,
    })
  })
})

describe('records query serialization', () => {
  it('encodes search text and preserves repeated values and all query settings', () => {
    const original = structuredClone(query)
    const params = convertQueryToSearchParams(query)

    expect(params.toString()).toContain('search=medicine+%26+vaccine')
    expect(params.getAll('recordType')).toEqual(['Medicine', 'Vaccine'])
    expect(params.getAll('recordStatus')).toEqual(['Active', 'OnHold'])
    expect(params.get('updateStatus')).toBe('NotOverdue')
    expect(params.get('page')).toBe('3')
    expect(params.get('pageSize')).toBe('25')
    expect(params.get('sortBy')).toBe('DevelopmentName')
    expect(params.get('sortDirection')).toBe('Descending')
    expect(query).toEqual(original)
  })

  it('omits absent values and empty arrays', () => {
    expect(
      convertQueryToSearchParams({
        search: undefined,
        recordType: [],
        recordStatus: [],
      }).toString(),
    ).toBe('')
  })

  it('round trips a fully populated query', () => {
    const params = convertQueryToSearchParams(query)

    expect(
      parseQueryFromSearchParams({
        ...Object.fromEntries(params),
        recordType: params.getAll('recordType'),
        recordStatus: params.getAll('recordStatus'),
      }),
    ).toEqual(query)
  })
})
