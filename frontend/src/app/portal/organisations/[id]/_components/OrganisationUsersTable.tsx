import Link from 'next/link'

import { Button } from '@nice-digital/nds-button'
import { EnhancedPagination } from '@nice-digital/nds-enhanced-pagination'
import { FilterSummary } from '@nice-digital/nds-filters'
import { Grid, GridItem } from '@nice-digital/nds-grid'

import {
  lastActivePresetDays,
  roleLabels,
  statusLabels,
  statusTagColours,
  type LastActivePreset,
} from '@/app/portal/_constants/userLabels'
import { getUsers } from '@/client/generated/sdk.gen'
import type { UserListItemDto, UserOrgStatus } from '@/client/generated/types.gen'
import { Table } from '@/components/Table/Table'
import { Tag } from '@/components/Tag/Tag'

import styles from '../page.module.scss'

import type { ComponentProps } from 'react'

interface OrganisationUsersTableProps {
  currentPage: number
  organisationId: number
  pageSize: number
  status: UserOrgStatus[]
  email?: string
  lastActive?: LastActivePreset
}

const resultsPerPage = [10, 25, 50]

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
        <>
          <a>Approve</a>
          <br></br>
          <a>Reject</a>
        </>
      )
    default:
      return 'Not applicable'
  }
}

function getFirstResult(
  totalCount: number | string,
  currentPage: number,
  pageSize: number,
): number {
  return Number(totalCount) === 0 ? 0 : (currentPage - 1) * pageSize + 1
}

function getLastResult(totalCount: number | string, currentPage: number, pageSize: number): number {
  return Math.min(currentPage * pageSize, Number(totalCount))
}

function getTotalPages(totalCount: number | string, pageSize: number): number {
  return Math.ceil(Number(totalCount) / pageSize)
}

function buildHref(
  page: number,
  pageSize: number,
  status: UserOrgStatus[],
  email: string | undefined,
  lastActive: LastActivePreset | undefined,
): string {
  const params = new URLSearchParams()
  params.set('page', String(page))
  params.set('pageSize', String(pageSize))
  status.forEach((value) => params.append('status', value))
  if (email) {
    params.set('email', email)
  }
  if (lastActive) {
    params.set('lastActive', lastActive)
  }

  return `?${params.toString()}`
}

function getLastActiveFromDate(preset: LastActivePreset): string {
  const days = lastActivePresetDays[preset]

  return new Date(Date.now() - days * 24 * 60 * 60 * 1000).toISOString()
}

export async function OrganisationUsersTable({
  currentPage,
  organisationId,
  pageSize,
  status,
  email,
  lastActive,
}: OrganisationUsersTableProps) {
  const { data: users, error: usersError } = await getUsers({
    query: {
      OrganisationId: organisationId,
      Page: currentPage,
      PageSize: pageSize,
      Status: status.length ? status : undefined,
      Email: email,
      LastActiveFrom: lastActive ? getLastActiveFromDate(lastActive) : undefined,
    },
  })

  return (
    <>
      <div className={styles['table-toolbar']}>
        <FilterSummary className={styles['users-filter-summary']}>
          {users
            ? `Showing results ${getFirstResult(
                users.totalCount,
                currentPage,
                pageSize,
              )} to ${getLastResult(users.totalCount, currentPage, pageSize)} of ${users.totalCount}`
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
                currentPage={currentPage}
                elementType={PaginationLink}
                mapPageNumberToHref={(pageNumber) =>
                  buildHref(pageNumber, pageSize, status, email, lastActive)
                }
                totalPages={getTotalPages(users.totalCount, pageSize)}
              />
            </GridItem>
            <GridItem cols={12} sm={6} className="text-right">
              <p className={styles.resultsPerPageHeading}>Results per page</p>
              <ol className={`list list--piped ${styles.resultsPerPageList}`}>
                {resultsPerPage.map((count) => (
                  <li key={count}>
                    {pageSize === count ? (
                      count
                    ) : (
                      <PaginationLink href={buildHref(1, count, status, email, lastActive)}>
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
