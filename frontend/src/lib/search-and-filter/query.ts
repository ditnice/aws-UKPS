import { SortDirection } from '@/client/generated'
import { TableSortDirection } from '@/components/Table/TableSortHeader'

export function parseMulti<T extends string>(
  param: string | string[] | undefined,
  validValues: readonly T[],
): T[] {
  const values = Array.isArray(param) ? param : param ? [param] : []

  return values.filter((value): value is T => validValues.includes(value as T))
}

export type GetNextSortDirectionProps<T> = {
  column: T
  sortBy: T | undefined
  sortDirection: SortDirection | undefined
}
export const getNextSortDirection = <T>(
  query: GetNextSortDirectionProps<T>,
): TableSortDirection => {
  if (query.sortBy === query.column && query.sortDirection) {
    return query.sortDirection == 'Ascending' ? 'ascending' : 'descending'
  }
  return 'none'
}

export const parseSortDirection = (
  sortDirection: string | undefined,
): SortDirection | undefined => {
  const validSortDirections: SortDirection[] = ['Ascending', 'Descending']
  return validSortDirections.includes(sortDirection as SortDirection)
    ? (sortDirection as SortDirection)
    : undefined
}
