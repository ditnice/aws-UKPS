import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { navigationState, router } from '@/test-utils/nextNavigation'
import { fillInput } from '@/test-utils/userInteractions'

import RecordsTablesFilters from './RecordsTablesFilters'

import type { RecordsQuery } from './recordsQuery'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
  navigationState.pathname = '/portal/organisations/2/records'
})

const query: RecordsQuery = {
  search: 'medicine',
  recordType: ['Medicine'],
  recordStatus: ['Active', 'OnHold'],
  updateStatus: 'Overdue',
  page: 3,
  pageSize: 25,
  sortBy: 'Id',
  sortDirection: 'Descending',
}

function expectNavigation(expected: RecordsQuery) {
  expect(router.push).toHaveBeenCalledOnce()
  const [href, options] = router.push.mock.calls[0]
  const url = new URL(href, 'https://example.test')
  expect(url.pathname).toBe('/portal/organisations/2/records')
  expect(options).toEqual({ scroll: false })
  const actual = Object.fromEntries(
    [...new Set(url.searchParams.keys())].map((key) => [key, url.searchParams.getAll(key)]),
  )
  const entries = Object.entries(expected)
    .filter(([, value]) => value !== undefined)
    .filter(([, value]) => !Array.isArray(value) || value.length > 0)
    .map(([key, value]) => [key, Array.isArray(value) ? value : [String(value)]])
  expect(actual).toEqual(Object.fromEntries(entries))
}

describe('RecordsTablesFilters', () => {
  it('renders no selections or search text for an empty query', () => {
    render(<RecordsTablesFilters query={{}} />)

    expect(screen.getByRole('textbox', { name: 'Search Records' })).toHaveValue('')
    for (const checkbox of screen.getAllByRole('checkbox')) {
      expect(checkbox).not.toBeChecked()
    }
  })

  it('reflects the selected statuses and initial search text', () => {
    render(<RecordsTablesFilters query={query} />)

    expect(screen.getByRole('textbox', { name: 'Search Records' })).toHaveValue('medicine')
    expect(screen.getByRole('checkbox', { name: 'Active' })).toBeChecked()
    expect(screen.getByRole('checkbox', { name: 'On Hold' })).toBeChecked()
    expect(screen.getByRole('checkbox', { name: 'Overdue' })).toBeChecked()
    expect(screen.getByRole('checkbox', { name: 'Not overdue' })).not.toBeChecked()
  })

  it('adds a record status, retaining existing filters and sorting and resetting the page', async () => {
    const original = structuredClone(query)
    render(<RecordsTablesFilters query={query} />)

    await user.click(screen.getByRole('checkbox', { name: 'Archived' }))

    expectNavigation({ ...query, recordStatus: ['Active', 'OnHold', 'Archived'], page: 1 })
    expect(query).toEqual(original)
  })

  it('removes only the unchecked record status', async () => {
    render(<RecordsTablesFilters query={query} />)

    await user.click(screen.getByRole('checkbox', { name: 'Active' }))

    expectNavigation({ ...query, recordStatus: ['OnHold'], page: 1 })
  })

  it('starts a new status selection when no statuses exist', async () => {
    render(<RecordsTablesFilters query={{}} />)

    await user.click(screen.getByRole('checkbox', { name: 'Unpublished' }))

    expectNavigation({ recordStatus: ['Unpublished'], page: 1 })
  })

  it('replaces the update status when a different option is selected', async () => {
    render(<RecordsTablesFilters query={query} />)

    await user.click(screen.getByRole('checkbox', { name: 'Not overdue' }))

    expectNavigation({ ...query, updateStatus: 'NotOverdue', page: 1 })
  })

  it('clears the selected update status when it is unchecked', async () => {
    render(<RecordsTablesFilters query={query} />)

    await user.click(screen.getByRole('checkbox', { name: 'Overdue' }))

    expectNavigation({ ...query, updateStatus: undefined, page: 1 })
  })

  it('trims and encodes search text without losing the other query settings', async () => {
    render(<RecordsTablesFilters query={query} />)

    await fillInput(
      user,
      screen.getByRole('textbox', { name: 'Search Records' }),
      '  vaccine & medicine  ',
    )
    await user.click(screen.getByRole('button', { name: 'Apply filter' }))

    expectNavigation({ ...query, search: 'vaccine & medicine', page: 1 })
  })

  it('removes the last status from the URL when it is unchecked', async () => {
    render(<RecordsTablesFilters query={{ recordStatus: ['Active'] }} />)

    await user.click(screen.getByRole('checkbox', { name: 'Active' }))

    expectNavigation({ page: 1 })
  })
})
