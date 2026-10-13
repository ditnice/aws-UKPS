import { describe, expect, it } from 'vitest'

import { lastActiveLabels, roleLabels, statusLabels } from './userLabels'
import {
  buildUserListHref,
  buildUserListSearchParams,
  getActiveFilters,
  getNumberOfActiveFilters,
  getUpdatedQueryWithoutFilter,
  parseUserListQuery,
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
    const original = structuredClone(query)

    getUpdatedQueryWithoutFilter(query, {
      key: 'status',
      value: 'Active',
      label: statusLabels.Active,
    })

    expect(query).toEqual(original)
  })
})

describe('parseUserListQuery', () => {
  it('returns defaults for an empty query', () => {
    expect(parseUserListQuery({})).toEqual({
      page: 1,
      pageSize: 10,
      status: [],
      role: [],
      email: undefined,
      lastActive: undefined,
      sortBy: 'LastActive',
      sortDirection: 'Descending',
    })
  })

  it('parses valid filters, pagination, and sorting', () => {
    expect(
      parseUserListQuery({
        page: '2',
        pageSize: '25',
        status: ['Active', 'Inactive'],
        role: ['Champion', 'Standard'],
        email: '  user@example.com  ',
        lastActive: 'month',
        sortBy: 'Email',
        sortDirection: 'Ascending',
      }),
    ).toEqual({
      page: 2,
      pageSize: 25,
      status: ['Active', 'Inactive'],
      role: ['Champion', 'Standard'],
      email: 'user@example.com',
      lastActive: 'month',
      sortBy: 'Email',
      sortDirection: 'Ascending',
    })
  })

  it('parses scalar status and all supported last-active presets and sort values', () => {
    expect(parseUserListQuery({ status: 'Active' }).status).toEqual(['Active'])
    for (const lastActive of ['week', 'month', '6months', 'year'] as const) {
      expect(parseUserListQuery({ lastActive }).lastActive).toBe(lastActive)
    }
    for (const sortBy of ['Email', 'Role', 'Status', 'LastActive'] as const) {
      expect(parseUserListQuery({ sortBy }).sortBy).toBe(sortBy)
    }
    expect(parseUserListQuery({ sortDirection: 'Ascending' }).sortDirection).toBe('Ascending')
    expect(parseUserListQuery({ sortDirection: 'Descending' }).sortDirection).toBe('Descending')
  })

  it('ignores blank email and unknown last-active presets', () => {
    for (const email of ['', '   ']) {
      expect(parseUserListQuery({ email }).email).toBeUndefined()
    }
    expect(parseUserListQuery({ lastActive: 'unknown' }).lastActive).toBeUndefined()
  })

  it('filters unsupported values and defaults invalid pagination and sorting', () => {
    expect(
      parseUserListQuery({
        status: ['Active', 'Rejected'],
        role: ['Champion', 'Super'],
        page: '0',
        pageSize: '20',
        sortBy: 'unknown',
        sortDirection: 'unknown',
      }),
    ).toMatchObject({
      status: ['Active'],
      role: ['Champion'],
      page: 1,
      pageSize: 10,
      sortBy: 'LastActive',
      sortDirection: 'Descending',
    })
  })
})

describe('buildUserListSearchParams', () => {
  it('always includes pagination and repeats status and role filters', () => {
    const params = buildUserListSearchParams({
      ...emptyQuery,
      page: 2,
      status: ['Active', 'Inactive'],
      role: ['Champion', 'Standard'],
    })

    expect(params.get('page')).toBe('2')
    expect(params.get('pageSize')).toBe('20')
    expect(params.getAll('status')).toEqual(['Active', 'Inactive'])
    expect(params.getAll('role')).toEqual(['Champion', 'Standard'])
  })

  it('includes optional values when set and omits them otherwise', () => {
    const params = buildUserListSearchParams({
      ...emptyQuery,
      email: 'user@example.com',
      lastActive: 'week',
      sortBy: 'Email',
      sortDirection: 'Ascending',
    })
    expect(params.get('email')).toBe('user@example.com')
    expect(params.get('lastActive')).toBe('week')
    expect(params.get('sortBy')).toBe('Email')
    expect(params.get('sortDirection')).toBe('Ascending')

    const absent = buildUserListSearchParams(emptyQuery)
    for (const key of ['email', 'lastActive', 'sortBy', 'sortDirection']) {
      expect(absent.has(key)).toBe(false)
    }
  })
})

describe('buildUserListHref', () => {
  it('returns a leading question mark and URL-encodes email values', () => {
    const href = buildUserListHref({
      ...emptyQuery,
      email: 'user+tag@example.com',
    })

    expect(href.startsWith('?')).toBe(true)
    expect(href).toContain('email=user%2Btag%40example.com')
    expect(new URLSearchParams(href.slice(1)).get('email')).toBe('user+tag@example.com')
  })
})
