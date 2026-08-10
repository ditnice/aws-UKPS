import Link from 'next/link'

import { Button } from '@nice-digital/nds-button'
import { EnhancedPagination } from '@nice-digital/nds-enhanced-pagination'
import { FilterSummary } from '@nice-digital/nds-filters'
import { Grid, GridItem } from '@nice-digital/nds-grid'

import { pageSizeOptions } from '@/app/portal/_constants/pagination'
import {
  lastActivePresetDays,
  roleLabels,
  statusLabels,
  statusTagColours,
  type LastActivePreset,
} from '@/app/portal/_constants/userLabels'
import { buildUserListHref, type UserListQuery } from '@/app/portal/_utils/userListQuery'
import type { Client } from '@/client/generated/client'
import { getUsers } from '@/client/generated/sdk.gen'
import type { UserListItemDto } from '@/client/generated/types.gen'
import { Table } from '@/components/Table/Table'
import { Tag } from '@/components/Tag/Tag'

import styles from '../page.module.scss'

import type { ComponentProps } from 'react'

interface OrganisationUsersTableProps {
  apiClient: Client
  organisationId: number
  query: UserListQuery
}

function PaginationLink({ children, ...props }: ComponentProps<typeof Link>) {
  return (
    <Link {...props} scroll={false}>
      {children}
    </Link>
  )
}

function formatDate(date: string | null | undefined): string {
  if (!date) {
    return 'N/A'
  }

  return new Intl.DateTimeFormat('en-GB').format(new Date(date))
}

function renderStatus(status: UserListItemDto['status']) {
  const label = status ? statusLabels[status] : 'N/A'

  return status ? <Tag colour={statusTagColours[status]}>{label}</Tag> : <Tag>{label}</Tag>
}

function renderActions(status: UserListItemDto['status']) {
  switch (status) {
    case 'Active':
    case 'Inactive':
      return <a>Edit role</a>
    case 'Deactivated':
      return <a>Reactivate</a>
    case 'RequestedAccess':
      return (
        <ul className={styles.actionList}>
          <li>
            <a>Approve</a>
          </li>
          <li>
            <a>Reject</a>
          </li>
        </ul>
      )
    default:
      return 'Not applicable'
  }
}

function getFirstResult(totalCount: number, currentPage: number, pageSize: number): number {
  return totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1
}

function getLastResult(totalCount: number, currentPage: number, pageSize: number): number {
  return Math.min(currentPage * pageSize, totalCount)
}

function getTotalPages(totalCount: number, pageSize: number): number {
  return Math.ceil(totalCount / pageSize)
}

function getLastActiveFromDate(preset: LastActivePreset): string {
  const days = lastActivePresetDays[preset]

  return new Date(Date.now() - days * 24 * 60 * 60 * 1000).toISOString()
}

export async function OrganisationUsersTable({
  apiClient,
  organisationId,
  query,
}: OrganisationUsersTableProps) {
  const { page, pageSize, status, role, email, lastActive } = query

  const { data: users, error: usersError } = await getUsers({
    client: apiClient,
    query: {
      OrganisationId: organisationId,
      Page: page,
      PageSize: pageSize,
      Status: status.length ? status : undefined,
      Role: role.length ? role : undefined,
      Email: email,
      LastActiveFrom: lastActive ? getLastActiveFromDate(lastActive) : undefined,
    },
  })

  const totalCount = users ? Number(users.totalCount) : 0

  return (
    <>
      <div className={styles['table-toolbar']}>
        <FilterSummary className={styles['users-filter-summary']}>
          {users
            ? `Showing results ${getFirstResult(totalCount, page, pageSize)} to ${getLastResult(totalCount, page, pageSize)} of ${totalCount}`
            : 'Showing results'}
        </FilterSummary>
        <Button>Add a new user</Button>
      </div>
      {usersError || !users ? (
        <p role="alert">There was a problem retrieving the users. Please try again later.</p>
      ) : (
        <>
          <Table columnWidth="content">
            <caption className="visually-hidden">Organisation Users</caption>
            <thead>
              <tr>
                <th scope="col">Email address</th>
                <th scope="col">Role</th>
                <th scope="col">Status</th>
                <th scope="col">Last active</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.items.length > 0 ? (
                users.items.map((user) => (
                  <tr key={user.userId}>
                    <td>{user.emailAddress ?? 'N/A'}</td>
                    <td>{user.role ? roleLabels[user.role] : 'N/A'}</td>
                    <td>{renderStatus(user.status)}</td>
                    <td>{formatDate(user.lastActive)}</td>
                    <td>{renderActions(user.status)}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5}>No users found for this organisation.</td>
                </tr>
              )}
            </tbody>
          </Table>

          <Grid verticalAlignment="middle">
            <GridItem cols={12} sm={6}>
              <EnhancedPagination
                currentPage={page}
                elementType={PaginationLink}
                mapPageNumberToHref={(pageNumber) =>
                  buildUserListHref({ ...query, page: pageNumber })
                }
                totalPages={getTotalPages(totalCount, pageSize)}
              />
            </GridItem>
            <GridItem cols={12} sm={6} className="text-right">
              <p className={styles.resultsPerPageHeading}>Results per page</p>
              <ol className={`list list--piped ${styles.resultsPerPageList}`}>
                {pageSizeOptions.map((count) => (
                  <li key={count}>
                    {pageSize === count ? (
                      count
                    ) : (
                      <PaginationLink
                        href={buildUserListHref({ ...query, page: 1, pageSize: count })}
                      >
                        {count}
                      </PaginationLink>
                    )}
                  </li>
                ))}
              </ol>
            </GridItem>
          </Grid>
        </>
      )}
    </>
  )
}
