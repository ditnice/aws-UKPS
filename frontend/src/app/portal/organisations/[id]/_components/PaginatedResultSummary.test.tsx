import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { PaginatedResultSummary } from './PaginatedResultSummary'

describe('PaginatedResultSummary', () => {
  it('shows a neutral summary when a result is unavailable', () => {
    render(<PaginatedResultSummary />)
    expect(screen.getByText('Showing results')).toBeInTheDocument()
  })

  it.each([
    { totalCount: 0, page: 1, pageSize: 10, first: 0, last: 0 },
    { totalCount: 1, page: 1, pageSize: 10, first: 1, last: 1 },
    { totalCount: 35, page: 1, pageSize: 10, first: 1, last: 10 },
    { totalCount: 35, page: 2, pageSize: 10, first: 11, last: 20 },
    { totalCount: 35, page: 4, pageSize: 10, first: 31, last: 35 },
    { totalCount: 50, page: 2, pageSize: 25, first: 26, last: 50 },
  ])('shows $first to $last of $totalCount results', ({ first, last, ...result }) => {
    render(<PaginatedResultSummary result={{ ...result, items: [] }} />)
    expect(
      screen.getByText(`Showing results ${first} to ${last} of ${result.totalCount}`),
    ).toBeInTheDocument()
  })
})
