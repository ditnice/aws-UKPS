import { SortDirection } from '@/client/generated'
import { Table } from '@/components/Table/Table'
import { ActiveSortDirection, TableSortHeaderLink } from '@/components/Table/TableSortHeader'
import { getNextSortDirection } from '@/lib/search-and-filter/query'

import { ApplicationTablePagination } from './ApplicationTablePagination'

type SortingQuery<TSortValue> = { sortBy?: TSortValue; sortDirection?: SortDirection }

type BaseApplicationTableProps<
  TItem,
  TSortValue,
  TKey extends string,
  TQuery extends SortingQuery<TSortValue>,
> = {
  headers: { key: TKey; label: string; sortColumn: TSortValue }[]
  captionName: string
  query: TQuery
  getData: (key: TKey, item: TItem) => React.ReactElement
  queryToSearchParams: (query: TQuery) => URLSearchParams
  getItemKey: (item: TItem) => number | string | null
  fallbackText: string
}

type ApplicationTableWithPaginationProps<
  TItem,
  TSortValue,
  TKey extends string,
  TQuery extends SortingQuery<TSortValue>,
> = BaseApplicationTableProps<TItem, TSortValue, TKey, TQuery> & {
  result: {
    items: Array<TItem>
    totalCount: number
    page: number
    pageSize: number
  }
}
export const ApplicationTableWithPagination = <
  TItem,
  TSortValue,
  TKey extends string,
  TQuery extends SortingQuery<TSortValue>,
>(
  props: ApplicationTableWithPaginationProps<TItem, TSortValue, TKey, TQuery>,
) => {
  return (
    <>
      <ApplicationTable {...{ ...props, items: props.result.items }} />
      <ApplicationTablePagination {...props} />
    </>
  )
}

type ApplicationTableProps<
  TItem,
  TSortValue,
  TKey extends string,
  TQuery extends { sortBy?: TSortValue; sortDirection?: SortDirection },
> = BaseApplicationTableProps<TItem, TSortValue, TKey, TQuery> & {
  items: TItem[]
}

export const ApplicationTable = <
  TItem,
  TSortValue,
  TKey extends string,
  TQuery extends { sortBy?: TSortValue; sortDirection?: SortDirection },
>({
  items,
  captionName,
  query,
  headers,
  getData,
  queryToSearchParams,
  getItemKey,
  fallbackText,
}: ApplicationTableProps<TItem, TSortValue, TKey, TQuery>) => {
  const createSortHref = (sortValue: TSortValue) => {
    return (sortDirection: ActiveSortDirection) => {
      const searchParams = queryToSearchParams({
        ...query,
        SortBy: sortValue,
        SortDirection: sortDirection == 'ascending' ? 'Ascending' : 'Descending',
      })
      return `?${searchParams.toString()}`
    }
  }
  return (
    <>
      <Table columnWidth="content">
        <caption className="visually-hidden">{captionName}</caption>
        <thead>
          <tr>
            {headers.map(({ label, sortColumn }) =>
              sortColumn ? (
                <TableSortHeaderLink
                  key={label}
                  direction={getNextSortDirection<TSortValue>({
                    column: sortColumn,
                    sortBy: query.sortBy,
                    sortDirection: query.sortDirection,
                  })}
                  createHref={createSortHref(sortColumn)}
                >
                  {label}
                </TableSortHeaderLink>
              ) : (
                <th scope="col" key={label}>
                  {label}
                </th>
              ),
            )}
          </tr>
        </thead>
        <tbody>
          {items.length > 0 ? (
            items.map((x) => (
              <tr key={getItemKey(x)}>
                {headers.map((h) => (
                  <td key={h.key}>{getData(h.key, x)}</td>
                ))}
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan={headers.length}>{fallbackText}</td>
            </tr>
          )}
        </tbody>
      </Table>
    </>
  )
}
