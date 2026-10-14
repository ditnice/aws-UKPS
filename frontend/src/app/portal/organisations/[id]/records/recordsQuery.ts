import {
  GetRecordsQuerySortValue,
  RecordDisplayStatus,
  RecordType,
  SortDirection,
  UpdateStatus,
} from '@/client/generated'
import { parsePage, parsePageSize } from '@/lib/search-and-filter/pagination'
import { parseMulti, parseSortDirection } from '@/lib/search-and-filter/query'

import { recordStatusLabels, updateStatusLabels } from './labels'

type Filter = (
  | { key: 'search'; value: string }
  | {
      key: 'record-status'
      value: RecordDisplayStatus
    }
  | { key: 'update-status'; value: UpdateStatus }
) & { label: string }

export type OrganisationRecordsSearchParams = {
  search?: string
  recordType?: string | string[]
  recordStatus?: string | string[]
  updateStatus?: string | string[]
  page?: string
  pageSize?: string
  sortBy?: string
  sortDirection?: string
}

export type RecordsQuery = {
  search?: string
  recordType?: Array<RecordType>
  recordStatus?: Array<RecordDisplayStatus>
  updateStatus?: Array<UpdateStatus>
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
    ...(query.updateStatus?.map((status) => ({
      key: 'update-status',
      value: status,
      label: updateStatusLabels[status],
    })) ?? []),
  ]
}

export const buildQueryFromFilters = (
  filters: Filter[],
  initialQuery: RecordsQuery,
): RecordsQuery => ({
  ...initialQuery,
  search: filters.find((f) => f.key === 'search')?.value,
  recordStatus: filters.filter((f) => f.key === 'record-status').map((x) => x.value),
  updateStatus: filters.filter((f) => f.key === 'update-status').map((x) => x.value),
})

export const parseQueryFromSearchParams = (
  searchParams: OrganisationRecordsSearchParams,
): RecordsQuery => {
  return {
    search: searchParams.search,
    recordStatus: parseMulti(searchParams.recordStatus, Object.values(RecordDisplayStatus)),
    updateStatus: parseMulti(searchParams.updateStatus, Object.values(UpdateStatus)),
    recordType: parseMulti(searchParams.recordType, Object.values(RecordType)),
    page: parsePage(searchParams.page),
    pageSize: parsePageSize(searchParams.pageSize),
    sortBy: Object.values(GetRecordsQuerySortValue).includes(
      searchParams.sortBy as GetRecordsQuerySortValue,
    )
      ? (searchParams.sortBy as GetRecordsQuerySortValue)
      : 'DisplayStatus',
    sortDirection: parseSortDirection(searchParams.sortDirection) ?? 'Ascending',
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
