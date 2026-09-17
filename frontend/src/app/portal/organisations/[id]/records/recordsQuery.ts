import {
  GetRecordsQuerySortValue,
  RecordStatus,
  RecordType,
  SortDirection,
} from '@/client/generated'
import { parsePage, parsePageSize } from '@/lib/search-and-filter/pagination'
import { parseMulti, parseSortDirection } from '@/lib/search-and-filter/query'

import { recordStatusLabels } from './labels'

type Filter = (
  | { key: 'search'; value: string }
  | {
      key: 'record-status'
      value: RecordStatus
    }
) & { label: string }

export type OrganisationRecordsSearchParams = {
  search?: string
  recordType?: string | string[]
  recordStatus?: string | string[]
  page?: string
  pageSize?: string
  sortBy?: string
  sortDirection?: string
}

export type RecordsQuery = {
  search?: string
  recordType?: Array<RecordType>
  recordStatus?: Array<RecordStatus>
  page?: number
  pageSize?: number
  sortBy?: GetRecordsQuerySortValue
  sortDirection?: SortDirection
}

export const getActiveFilters = (query: RecordsQuery): Filter[] => {
  return [
    ...(query.search ? [{ key: 'search', value: query.search, label: query.search } as const] : []),
    ...(query.recordStatus?.map(
      (s) =>
        ({
          key: 'record-status',
          value: s,
          label: recordStatusLabels[s],
        }) as const,
    ) ?? []),
  ]
}

export const buildQueryFromFilters = (
  filters: Filter[],
  initialQuery: RecordsQuery,
): RecordsQuery => ({
  ...initialQuery,
  search: filters.find((f) => f.key === 'search')?.value,
  recordStatus: filters.filter((f) => f.key === 'record-status').map((x) => x.value),
})

export const parseQueryFromSearchParams = (
  searchParams: OrganisationRecordsSearchParams,
): RecordsQuery => {
  return {
    search: searchParams.search,
    recordStatus: parseMulti(searchParams.recordStatus, Object.values(RecordStatus)),
    recordType: parseMulti(searchParams.recordType, Object.values(RecordType)),
    page: parsePage(searchParams.page),
    pageSize: parsePageSize(searchParams.pageSize),
    sortBy: Object.values(GetRecordsQuerySortValue).includes(
      searchParams.sortBy as GetRecordsQuerySortValue,
    )
      ? (searchParams.sortBy as GetRecordsQuerySortValue)
      : undefined,
    sortDirection: parseSortDirection(searchParams.sortDirection),
  }
}

const toLowerCamelCase = (value: string) => {
  return value.replace(/^[A-Z]/, (match) => match.toLowerCase())
}

export const convertQueryToSearchParams = (query: RecordsQuery): URLSearchParams => {
  const searchParams = new URLSearchParams()

  for (const [key, value] of Object.entries(query)) {
    if (value == null) continue

    const lowerCamelCaseKey = toLowerCamelCase(key)
    if (Array.isArray(value)) {
      value.forEach((v) => searchParams.append(lowerCamelCaseKey, String(v)))
    } else {
      searchParams.set(toLowerCamelCase(lowerCamelCaseKey), String(value))
    }
  }

  return searchParams
}
