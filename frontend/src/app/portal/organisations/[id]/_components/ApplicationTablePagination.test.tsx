import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { buildUserListSearchParams, parseUserListQuery } from '../_lib/userListQuery'

import { ApplicationTablePagination } from './ApplicationTablePagination'

const query = parseUserListQuery({
  email: 'jane+tag@example.com',
  status: ['Active', 'Inactive'],
  role: 'Champion',
  page: '2',
  pageSize: '10',
  sortBy: 'Email',
  sortDirection: 'Ascending',
})

function renderPagination(
  overrides: Partial<{ totalCount: number; page: number; pageSize: number }> = {},
) {
  render(
    <ApplicationTablePagination
      result={{ totalCount: 35, page: 2, pageSize: 10, ...overrides }}
      query={query}
      queryToSearchParams={buildUserListSearchParams}
    />,
  )
}

function getLinkParams(link: HTMLElement) {
  return new URL(link.getAttribute('href')!, 'https://example.test').searchParams
}

describe('ApplicationTablePagination', () => {
  it('shows the rounded-up page count and marks the current page without a link', () => {
    renderPagination()
    const navigation = screen.getByRole('navigation', { name: 'Pagination' })

    expect(within(navigation).getByText('Current page')).toBeInTheDocument()
    expect(within(navigation).getByText('Current page').closest('li')).toHaveAttribute(
      'aria-current',
      'true',
    )
    expect(within(navigation).queryByRole('link', { name: 'Go to page 2' })).not.toBeInTheDocument()
    expect(navigation).toHaveTextContent('Page 2 of 4')
  })

  it.each([
    ['Previous page', '1'],
    ['Next page', '3'],
    ['Go to page 4', '4'],
  ])('preserves filters and sorting in the %s link', (name, page) => {
    renderPagination()
    const params = getLinkParams(screen.getByRole('link', { name }))

    expect(params.get('page')).toBe(page)
    expect(params.get('pageSize')).toBe('10')
    expect(params.get('email')).toBe('jane+tag@example.com')
    expect(params.getAll('status')).toEqual(['Active', 'Inactive'])
    expect(params.getAll('role')).toEqual(['Champion'])
    expect(params.get('sortBy')).toBe('Email')
    expect(params.get('sortDirection')).toBe('Ascending')
  })

  it.each(['25', '50'])('resets to page one when switching to %s results per page', (size) => {
    renderPagination()
    const params = getLinkParams(screen.getByRole('link', { name: size }))

    expect(params.get('page')).toBe('1')
    expect(params.get('pageSize')).toBe(size)
    expect(params.get('email')).toBe('jane+tag@example.com')
    expect(params.getAll('status')).toEqual(['Active', 'Inactive'])
    expect(params.get('sortBy')).toBe('Email')
    expect(screen.queryByRole('link', { name: '10' })).not.toBeInTheDocument()
  })

  it('omits previous navigation on the first page', () => {
    renderPagination({ page: 1 })
    expect(screen.queryByRole('link', { name: 'Previous page' })).not.toBeInTheDocument()
  })

  it('omits next navigation on the last page', () => {
    renderPagination({ page: 4 })
    expect(screen.queryByRole('link', { name: 'Next page' })).not.toBeInTheDocument()
  })
})
