import {
  GetRecordsQuerySortValue,
  RecordStatus,
  RecordType,
  SortDirection,
} from '@/client/generated'
import { parsePage, parsePageSize } from '@/lib/search-and-filter/pagination'
import { parseMulti } from '@/lib/search-and-filter/query'

import { recordStatusLabels } from './labels'

type Filter = (
  | { key: 'search'; value: string }
  | {
      key: 'record-status'
      value: RecordStatus
    }
) & { label: string }
export const getActiveFilters = (query: RecordsQuery): Filter[] => {
  return [
    ...(query.Search ? [{ key: 'search', value: query.Search, label: query.Search } as const] : []),
    ...(query.RecordStatus?.map(
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
  Search: filters.find((f) => f.key === 'search')?.value,
  RecordStatus: filters.filter((f) => f.key === 'record-status').map((x) => x.value),
})

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
  Search?: string
  RecordType?: Array<RecordType>
  RecordStatus?: Array<RecordStatus>
  Page?: number
  PageSize?: number
  SortBy?: GetRecordsQuerySortValue
  SortDirection?: SortDirection
}

export const parseQueryFromSearchParams = (
  searchParams: OrganisationRecordsSearchParams,
): RecordsQuery => {
  return {
    Search: searchParams.search,
    RecordStatus: parseMulti(searchParams.recordStatus, Object.values(RecordStatus)),
    RecordType: parseMulti(searchParams.recordType, Object.values(RecordType)),
    Page: parsePage(searchParams.page),
    PageSize: parsePageSize(searchParams.pageSize),
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
