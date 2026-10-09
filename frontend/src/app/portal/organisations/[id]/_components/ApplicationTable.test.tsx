import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import type { GetUsersQuerySortValue } from '@/client/generated'

import { buildUserListSearchParams, parseUserListQuery } from '../_lib/userListQuery'
import { convertQueryToSearchParams } from '../records/recordsQuery'

import { ApplicationTable, ApplicationTableWithPagination } from './ApplicationTable'

import type { UserListQuery } from '../_lib/userListQuery'

const items = [
  { id: 4, email: 'jane@example.com', role: 'Champion user' },
  { id: 5, email: 'sam@example.com', role: 'Standard user' },
]
type Item = (typeof items)[number]
type Column = 'email' | 'role'
const headers: { key: Column; label: string; sortColumn: GetUsersQuerySortValue | null }[] = [
  { key: 'email', label: 'Email address', sortColumn: 'Email' },
  { key: 'role', label: 'Role', sortColumn: null },
]

function tableProps(query: UserListQuery = parseUserListQuery({})) {
  return {
    captionName: 'Organisation users',
    headers,
    query,
    queryToSearchParams: buildUserListSearchParams,
    getItemKey: (item: Item) => item.id,
    getData: (key: Column, item: Item) => <>{item[key]}</>,
    fallbackText: 'No users match the selected filters',
  }
}

function getSortParams() {
  const link = screen.getByRole('link', { name: 'Email address' })
  return new URL(link.getAttribute('href')!, 'https://example.test').searchParams
}

describe('ApplicationTable', () => {
  it('renders an accessible caption, column headings and each item in its own row', () => {
    render(<ApplicationTable {...tableProps()} items={items} />)
    const table = screen.getByRole('table', { name: 'Organisation users' })
    const rows = within(table).getAllByRole('row')

    expect(rows).toHaveLength(3)
    expect(within(rows[1]).getByRole('cell', { name: 'jane@example.com' })).toBeInTheDocument()
    expect(within(rows[1]).getByRole('cell', { name: 'Champion user' })).toBeInTheDocument()
    expect(within(rows[2]).getByRole('cell', { name: 'sam@example.com' })).toBeInTheDocument()
    expect(screen.getByRole('columnheader', { name: 'Role' })).toHaveAttribute('scope', 'col')
    expect(screen.queryByRole('link', { name: 'Role' })).not.toBeInTheDocument()
  })

  it('shows the empty-state message spanning all columns when no items match', () => {
    render(<ApplicationTable {...tableProps()} items={[]} />)

    expect(
      screen.getByRole('cell', { name: 'No users match the selected filters' }),
    ).toHaveAttribute('colspan', '2')
    expect(screen.getAllByRole('row')).toHaveLength(2)
  })

  it.each([
    { sortBy: 'LastActive', sortDirection: 'Descending', current: 'none', next: 'Ascending' },
    { sortBy: 'Email', sortDirection: 'Ascending', current: 'ascending', next: 'Descending' },
    { sortBy: 'Email', sortDirection: 'Descending', current: 'descending', next: 'Ascending' },
  ] as const)('sorts from $current to $next while retaining filters and pagination', (sort) => {
    const query = parseUserListQuery({
      email: 'jane+tag@example.com',
      status: ['Active', 'Inactive'],
      role: 'Champion',
      page: '2',
      pageSize: '25',
      sortBy: sort.sortBy,
      sortDirection: sort.sortDirection,
    })
    const original = structuredClone(query)
    render(<ApplicationTable {...tableProps(query)} items={items} />)

    expect(screen.getByRole('columnheader', { name: 'Email address' })).toHaveAttribute(
      'aria-sort',
      sort.current,
    )
    const params = getSortParams()
    expect(params.get('sortBy')).toBe('Email')
    expect(params.get('sortDirection')).toBe(sort.next)
    expect(params.getAll('sortBy')).toHaveLength(1)
    expect(params.getAll('sortDirection')).toHaveLength(1)
    expect(params.get('email')).toBe('jane+tag@example.com')
    expect(params.getAll('status')).toEqual(['Active', 'Inactive'])
    expect(params.getAll('role')).toEqual(['Champion'])
    expect(params.get('page')).toBe('2')
    expect(params.get('pageSize')).toBe('25')
    expect(query).toEqual(original)
  })

  it('builds record sort links without duplicate or incorrectly cased sort parameters', () => {
    render(
      <ApplicationTable
        items={[{ id: 42 }]}
        captionName="Organisation records"
        headers={[{ key: 'id', label: 'ID', sortColumn: 'Id' }]}
        query={{ sortBy: 'Id', sortDirection: 'Ascending', recordStatus: ['Active'], page: 2 }}
        queryToSearchParams={convertQueryToSearchParams}
        getData={(_key, item) => <>{item.id}</>}
        getItemKey={(item) => item.id}
        fallbackText="No records"
      />,
    )

    const href = screen.getByRole('link', { name: 'ID' }).getAttribute('href')!
    const params = new URL(href, 'https://example.test').searchParams
    expect(params.getAll('sortBy')).toEqual(['Id'])
    expect(params.getAll('sortDirection')).toEqual(['Descending'])
    expect(params.getAll('recordStatus')).toEqual(['Active'])
    expect(params.has('SortBy')).toBe(false)
    expect(params.has('SortDirection')).toBe(false)
  })

  it('composes the real table and pagination for paginated results', () => {
    render(
      <ApplicationTableWithPagination
        {...tableProps()}
        result={{ items, totalCount: 12, page: 1, pageSize: 10 }}
      />,
    )

    expect(screen.getByRole('table', { name: 'Organisation users' })).toBeInTheDocument()
    expect(screen.getByRole('navigation', { name: 'Pagination' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Next page' })).toBeInTheDocument()
  })
})
