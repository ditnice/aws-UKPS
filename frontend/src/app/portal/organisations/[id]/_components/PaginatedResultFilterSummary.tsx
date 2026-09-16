'use client'

import { useRouter } from 'next/navigation'

import { FilterSummary } from '@nice-digital/nds-filters'

import styles from './PaginatedResultFilterSummary.module.scss'

export type PaginatedResultFilterSummaryProps<
  T,
  TQuery,
  TFilter extends { key: string; value: string; label: string },
> = {
  result?: {
    items: Array<T>
    totalCount: number
    page: number
    pageSize: number
  }
  query: TQuery
  getActiveFilters: (query: TQuery) => TFilter[]
  convertFiltersToQuery: (filters: TFilter[], query: TQuery) => TQuery
  convertQueryToSearchParams: (query: TQuery) => URLSearchParams
}
export const PaginatedResultFilterSummary = <
  T,
  TQuery,
  TFilter extends { key: string; value: string; label: string },
>({
  result,
  query,
  getActiveFilters,
  convertFiltersToQuery,
  convertQueryToSearchParams,
}: PaginatedResultFilterSummaryProps<T, TQuery, TFilter>) => {
  const router = useRouter()
  const totalCount = result?.totalCount ?? 0
  const activeFilters = getActiveFilters(query)
  const activeFilterItems = activeFilters.map((af) => {
    const activeFiltersWithRemovedItem = activeFilters.filter(
      (f) => f.key !== af.key && f.value !== af.value,
    )
    const updatedQuery = convertFiltersToQuery(activeFiltersWithRemovedItem, query)
    const href = `?${convertQueryToSearchParams(updatedQuery).toString()}`
    return { ...af, onClick: () => router.push(href, { scroll: false }) }
  })

  const getFirstResult = (totalCount: number, currentPage: number, pageSize: number): number => {
    return totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1
  }

  const getLastResult = (totalCount: number, currentPage: number, pageSize: number): number => {
    return Math.min(currentPage * pageSize, totalCount)
  }

  return (
    <FilterSummary className={styles['users-filter-summary']} activeFilters={activeFilterItems}>
      {result
        ? `Showing results ${getFirstResult(totalCount, result.page, result.pageSize)} to ${getLastResult(totalCount, result.page, result.pageSize)} of ${totalCount}`
        : 'Showing results'}
    </FilterSummary>
  )
}
