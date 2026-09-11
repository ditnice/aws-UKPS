import { describe, expect, it } from 'vitest'

import { lastActiveLabels, roleLabels, statusLabels } from './userLabels'
import {
  getActiveFilters,
  getNumberOfActiveFilters,
  getUpdatedQueryWithoutFilter,
  UserListQuery,
} from './userListQuery'

const emptyQuery: UserListQuery = {
  status: [],
  role: [],
  email: undefined,
  lastActive: undefined,
  page: 1,
  pageSize: 20,
}

describe('getActiveFilters', () => {
  it('returns no filters when the query is empty', () => {
    expect(getActiveFilters(emptyQuery)).toEqual([])
  })

  it('returns status filters', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active'],
    }

    expect(getActiveFilters(query)).toEqual([
      {
        key: 'status',
        value: 'Active',
        label: statusLabels.Active,
      },
    ])
  })

  it('returns multiple status filters', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active', 'Inactive'],
    }

    expect(getActiveFilters(query)).toEqual([
      {
        key: 'status',
        value: 'Active',
        label: statusLabels.Active,
      },
      {
        key: 'status',
        value: 'Inactive',
        label: statusLabels.Inactive,
      },
    ])
  })

  it('returns role filters', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      role: ['Champion'],
    }

    expect(getActiveFilters(query)).toEqual([
      {
        key: 'role',
        value: 'Champion',
        label: roleLabels.Champion,
      },
    ])
  })

  it('returns an email filter', () => {
    const email = 'user@example.com'
    const query: UserListQuery = {
      ...emptyQuery,
      email,
    }

    expect(getActiveFilters(query)).toEqual([
      {
        key: 'email',
        value: email,
        label: email,
      },
    ])
  })

  it('does not return an email filter when email is empty', () => {
    const query: UserListQuery = {
      ...emptyQuery,
    }

    expect(getActiveFilters(query)).not.toContainEqual(
      expect.objectContaining({
        key: 'email',
      }),
    )
  })

  it('returns a last active filter', () => {
    const lastActive = '6months' as const
    const query: UserListQuery = {
      ...emptyQuery,
      lastActive,
    }

    expect(getActiveFilters(query)).toEqual([
      {
        key: 'last-active',
        value: lastActive,
        label: lastActiveLabels[lastActive],
      },
    ])
  })
})

describe('getNumberOfActiveFilters', () => {
  it('returns zero when there are no active filters', () => {
    expect(getNumberOfActiveFilters(emptyQuery)).toBe(0)
  })

  it('returns the number of active filters', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active', 'Inactive'],
      role: ['Champion'],
      email: 'user@example.com',
      lastActive: '6months',
    }

    expect(getNumberOfActiveFilters(query)).toBe(5)
  })

  it('counts each status and role as a separate filter', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active', 'Inactive'],
      role: ['Champion', 'Standard'],
    }

    expect(getNumberOfActiveFilters(query)).toBe(4)
  })
})

describe('getUpdatedQueryWithoutFilter', () => {
  it('removes a status filter', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active', 'Inactive'],
    }

    const result = getUpdatedQueryWithoutFilter(query, {
      key: 'status',
      value: 'Active',
      label: statusLabels.Active,
    })

    expect(result).toEqual({
      ...query,
      status: ['Inactive'],
    })
  })

  it('removes a role filter', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: [],
      role: ['Champion', 'Standard'],
    }

    const result = getUpdatedQueryWithoutFilter(query, {
      key: 'role',
      value: 'Standard',
      label: roleLabels.Standard,
    })

    expect(result).toEqual({
      ...query,
      role: ['Champion'],
    })
  })

  it('removes the email filter', () => {
    const email = 'user@example.com'
    const query: UserListQuery = {
      ...emptyQuery,
      email: email,
    }

    const result = getUpdatedQueryWithoutFilter(query, {
      key: 'email',
      value: email,
      label: email,
    })

    expect(result).toEqual({
      ...query,
      email: undefined,
    })
  })

  it('removes the last active filter', () => {
    const lastActive = '6months' as const
    const query: UserListQuery = {
      ...emptyQuery,
      lastActive: lastActive,
    }

    const result = getUpdatedQueryWithoutFilter(query, {
      key: 'last-active',
      value: lastActive,
      label: lastActiveLabels[lastActive],
    })

    expect(result).toEqual({
      ...query,
      lastActive: undefined,
    })
  })

  it('only removes the matching filter when multiple filters have the same key', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active', 'Inactive'],
    }

    const result = getUpdatedQueryWithoutFilter(query, {
      key: 'status',
      value: 'Active',
      label: statusLabels.Active,
    })

    expect(result.status).toEqual(['Inactive'])
  })

  it('preserves all other filters', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active'],
      role: ['Champion'],
      email: 'user@example.com',
      lastActive: '6months',
    }

    const result = getUpdatedQueryWithoutFilter(query, {
      key: 'role',
      value: 'Champion',
      label: roleLabels.Champion,
    })

    expect(result).toEqual({
      ...emptyQuery,
      status: ['Active'],
      role: [],
      email: 'user@example.com',
      lastActive: '6months',
    })
  })

  it('does not mutate the original query', () => {
    const query: UserListQuery = {
      ...emptyQuery,
      status: ['Active', 'Inactive'],
      role: ['Super'],
      email: 'user@example.com',
      lastActive: '6months',
    }

    getUpdatedQueryWithoutFilter(query, {
      key: 'status',
      value: 'Active',
      label: statusLabels.Active,
    })

    expect(query).toEqual(query)
  })
})
