import Link from 'next/link'

import type { Client } from '@/client/generated/client'
import { getUsers } from '@/client/generated/sdk.gen'
import type { UserListItemDto, UserMembershipAction } from '@/client/generated/types.gen'
import { Button } from '@/components/Button/Button'
import { Tag } from '@/components/Tag/Tag'

import {
  lastActivePresetDays,
  organisationUserTableHeaders,
  roleLabels,
  statusLabels,
  statusTagColours,
  type LastActivePreset,
} from '../_lib/userLabels'
import { buildUserListSearchParams, type UserListQuery } from '../_lib/userListQuery'
import styles from '../page.module.scss'

import { ApplicationTableWithPagination } from './ApplicationTable'
import { UserFilterSummary } from './UserFilterSummary'

interface OrganisationUsersTableProps {
  apiClient: Client
  organisationId: number
  query: UserListQuery
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

function renderActions(user: UserListItemDto, organisationId: number) {
  const editActivities: UserMembershipAction[] = ['EditUserRole', 'DeactivateMembership']

  const links: { key: string; label: string; href: string }[] = []

  if (user.actions.includes('ApproveMembership') && user.registrationRequestId) {
    links.push({
      key: 'approve',
      label: 'Approve',
      href: `/portal/organisations/${organisationId}/registration-requests/${user.registrationRequestId}/approve`,
    })
  }

  if (user.actions.includes('RejectMembership') && user.registrationRequestId) {
    links.push({
      key: 'reject',
      label: 'Reject',
      href: `/portal/organisations/${organisationId}/registration-requests/${user.registrationRequestId}/reject`,
    })
  }

  if (user.actions.includes('ReactivateMembership')) {
    links.push({
      key: 'reactivate',
      label: 'Reactivate',
      href: `/portal/organisations/${organisationId}/users/${user.userId}/reactivate`,
    })
  }

  if (user.actions.some((x) => editActivities.includes(x))) {
    links.push({
      key: 'edit',
      label: 'Edit',
      href: `/portal/organisations/${organisationId}/manage-user-access/${user.userId}`,
    })
  }

  return (
    <ul className={styles.actionList}>
      {links.length
        ? links.map((link) => (
            <li key={link.key}>
              <Link href={link.href}>{link.label}</Link>
            </li>
          ))
        : 'Not applicable'}
    </ul>
  )
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
  const { page, pageSize, status, role, email, lastActive, sortBy, sortDirection } = query

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
      SortBy: sortBy,
      SortDirection: sortDirection,
    },
  })

  return (
    <>
      <div className={styles['table-toolbar']}>
        <UserFilterSummary query={query} users={users} />
        {/* TODO - remove the elementType when the Button wrapper is merged */}
        <Button elementType={Link} href={`/portal/organisations/${organisationId}/onboard-user`}>
          Add a new user
        </Button>
      </div>
      {usersError || !users ? (
        <p role="alert">There was a problem retrieving the users. Please try again later.</p>
      ) : (
        <ApplicationTableWithPagination
          result={users}
          getItemKey={(x) => x.userId}
          headers={organisationUserTableHeaders}
          captionName={'Organisation Users'}
          query={query}
          queryToSearchParams={buildUserListSearchParams}
          fallbackText="No users found for this organisation."
          getData={(key, data) => {
            switch (key) {
              case 'actions':
                return <>{renderActions(data, organisationId)}</>
              case 'email':
                return <>{data.emailAddress ?? 'N/A'}</>
              case 'lastActive':
                return <>{formatDate(data.lastActive)}</>
              case 'role':
                return <>{data.role ? roleLabels[data.role] : 'N/A'}</>
              case 'status':
                return <>{renderStatus(data.status)}</>
            }
          }}
        />
      )}
    </>
  )
}
