import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { navigationState, router } from '@/test-utils/nextNavigation'
import { fillInput } from '@/test-utils/userInteractions'

import { parseUserListQuery } from '../_lib/userListQuery'

import { OrganisationFilters } from './OrganisationFilters'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
  navigationState.pathname = '/portal/organisations/2'
})

function renderFilters(search = '') {
  navigationState.searchParams = new URLSearchParams(search)
  const params = navigationState.searchParams
  render(
    <OrganisationFilters
      query={parseUserListQuery({
        ...Object.fromEntries(params),
        status: params.getAll('status'),
        role: params.getAll('role'),
      })}
    />,
  )
}

function expectNavigation(expected: Record<string, string | string[]>) {
  expect(router.push).toHaveBeenCalledOnce()
  const [href, options] = router.push.mock.calls[0]
  const url = new URL(href, 'https://example.test')
  expect(url.pathname).toBe('/portal/organisations/2')
  expect(options).toEqual({ scroll: false })
  const actual: Record<string, string[]> = {}
  for (const key of new Set(url.searchParams.keys())) {
    actual[key] = url.searchParams.getAll(key)
  }
  expect(actual).toEqual(
    Object.fromEntries(
      Object.entries(expected).map(([key, value]) => [key, Array.isArray(value) ? value : [value]]),
    ),
  )
}

describe('OrganisationFilters', () => {
  it('renders available filters without offering super users or rejected memberships', () => {
    renderFilters()

    expect(screen.getByRole('heading', { name: 'Filters' })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Filter users' })).toHaveValue('')
    expect(screen.getByRole('checkbox', { name: 'Champion user' })).not.toBeChecked()
    expect(screen.getByRole('checkbox', { name: 'Pending' })).not.toBeChecked()
    expect(screen.queryByRole('checkbox', { name: 'Super user' })).not.toBeInTheDocument()
    expect(screen.queryByRole('checkbox', { name: 'Rejected' })).not.toBeInTheDocument()
  })

  it('shows selected values and the total active filter count from the URL', () => {
    renderFilters(
      'status=Active&status=Inactive&role=Champion&lastActive=month&email=jane%40example.com',
    )

    expect(screen.getByRole('heading', { name: 'Filters (5 Active)' })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Filter users' })).toHaveValue('jane@example.com')
    for (const name of ['Active', 'Inactive', 'Champion user', 'In the last month']) {
      expect(screen.getByRole('checkbox', { name })).toBeChecked()
    }
  })

  it('adds a role without losing other selections, resets the page and preserves sorting', async () => {
    renderFilters(
      'role=Champion&status=Active&page=4&pageSize=25&sortBy=Email&sortDirection=Ascending',
    )

    await user.click(screen.getByRole('checkbox', { name: 'Standard user' }))

    expectNavigation({
      role: ['Champion', 'Standard'],
      status: 'Active',
      page: '1',
      pageSize: '25',
      sortBy: 'Email',
      sortDirection: 'Ascending',
    })
  })

  it('removes only the unchecked status and retains the other status', async () => {
    renderFilters('status=Active&status=Inactive&role=Champion&page=3')

    await user.click(screen.getByRole('checkbox', { name: 'Active' }))

    expectNavigation({ status: 'Inactive', role: 'Champion', page: '1' })
  })

  it('removes the role parameter when its last selection is unchecked', async () => {
    renderFilters('role=Champion&status=Active')

    await user.click(screen.getByRole('checkbox', { name: 'Champion user' }))

    expectNavigation({ status: 'Active', page: '1' })
  })

  it('replaces an existing last-active preset instead of appending another one', async () => {
    renderFilters('lastActive=month&status=Active&page=2')

    await user.click(screen.getByRole('checkbox', { name: 'In the last week' }))

    expectNavigation({ lastActive: 'week', status: 'Active', page: '1' })
  })

  it('clears the last-active preset when it is unchecked', async () => {
    renderFilters('lastActive=month&page=2')

    await user.click(screen.getByRole('checkbox', { name: 'In the last month' }))

    expectNavigation({ page: '1' })
  })

  it('trims and URL-encodes submitted email while preserving the other filters', async () => {
    renderFilters('status=Active&page=4')

    await fillInput(
      user,
      screen.getByRole('textbox', { name: 'Filter users' }),
      '  jane+tag@example.com  ',
    )
    await user.click(screen.getByRole('button', { name: 'Apply filter' }))

    expectNavigation({ status: 'Active', email: 'jane+tag@example.com', page: '1' })
  })

  it.each(['', '   '])('clears the email filter on a blank submission: %j', async (email) => {
    renderFilters('email=jane%40example.com&role=Champion&page=2')

    await fillInput(user, screen.getByRole('textbox', { name: 'Filter users' }), email)
    await user.click(screen.getByRole('button', { name: 'Apply filter' }))

    expectNavigation({ role: 'Champion', page: '1' })
  })
})
