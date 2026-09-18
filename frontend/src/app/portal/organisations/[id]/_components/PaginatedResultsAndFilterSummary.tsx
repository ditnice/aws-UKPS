'use client'

import { useRouter } from 'next/navigation'

import { FilterSummary } from '@nice-digital/nds-filters'

import styles from './PaginatedResultFilterSummary.module.scss'
import { PaginatedResultSummary } from './PaginatedResultSummary'

export type PaginatedResultsAndFilterSummaryProps<
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
export const PaginatedResultsAndFilterSummary = <
  T,
  TQuery,
  TFilter extends { key: string; value: string; label: string },
>({
  result,
  query,
  getActiveFilters,
  convertFiltersToQuery,
  convertQueryToSearchParams,
}: PaginatedResultsAndFilterSummaryProps<T, TQuery, TFilter>) => {
  const router = useRouter()
  const activeFilters = getActiveFilters(query)
  const activeFilterItems = activeFilters.map((af) => {
    const activeFiltersWithRemovedItem = activeFilters.filter(
      (f) => f.key !== af.key && f.value !== af.value,
    )
    const updatedQuery = convertFiltersToQuery(activeFiltersWithRemovedItem, query)
    const href = `?${convertQueryToSearchParams(updatedQuery).toString()}`
    return { ...af, onClick: () => router.push(href, { scroll: false }) }
  })

  return (
    <FilterSummary className={styles['users-filter-summary']} activeFilters={activeFilterItems}>
      <PaginatedResultSummary result={result} />
    </FilterSummary>
  )
}
