import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { UserOrgStatus, UserRole } from '@/client/generated/types.gen'
import { router } from '@/test-utils/nextNavigation'

import ManageUserAccessActions from './ManageUserAccessActions'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

function renderActions(
  status: UserOrgStatus = 'Active',
  currentUserRole: UserRole = 'Super',
  selectedUserRole: UserRole = 'Standard',
) {
  render(
    <ManageUserAccessActions
      organisationId={2}
      selectedUserId={4}
      status={status}
      currentUserRole={currentUserRole}
      selectedUserRole={selectedUserRole}
    />,
  )
}

describe('ManageUserAccessActions', () => {
  it('offers the available actions without selecting one by default', () => {
    renderActions()

    for (const name of ['Change user permissions', 'Deactivate user', 'Remove user']) {
      expect(screen.getByRole('radio', { name })).not.toBeChecked()
    }
  })

  it('offers only removal for an awaiting-setup user managed by a Super user', () => {
    renderActions('AwaitingSetup')
    expect(screen.getAllByRole('radio')).toHaveLength(1)
    expect(screen.getByRole('radio', { name: 'Remove user' })).toBeInTheDocument()
  })

  it('offers only reactivation for a deactivated user managed by a Champion', () => {
    renderActions('Deactivated', 'Champion')
    expect(screen.queryByRole('radio', { name: 'Remove user' })).not.toBeInTheDocument()
    expect(screen.queryByRole('radio', { name: 'Deactivate user' })).not.toBeInTheDocument()
    expect(screen.getByRole('radio', { name: 'Reactivate user' })).toBeInTheDocument()
  })

  it.each(['Removed', 'RequestedAccess', 'Rejected'] as const)(
    'does not render a form for %s users',
    (status) => {
      renderActions(status)
      expect(screen.getByText('No actions are available for this user.')).toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Continue' })).not.toBeInTheDocument()
    },
  )

  it('does not offer management to Standard users', () => {
    renderActions('Active', 'Standard')
    expect(screen.queryByRole('radio')).not.toBeInTheDocument()
  })

  it('does not offer removal to Champion users', () => {
    renderActions('Active', 'Champion')
    expect(screen.queryByRole('radio', { name: 'Remove user' })).not.toBeInTheDocument()
    expect(screen.getByRole('radio', { name: 'Deactivate user' })).toBeInTheDocument()
  })

  it('does not offer role changes for a Super user', () => {
    renderActions('Active', 'Super', 'Super')
    expect(screen.queryByRole('radio', { name: 'Change user permissions' })).not.toBeInTheDocument()
  })

  it('shows a selection error without navigating when Continue is clicked without an action', async () => {
    renderActions()

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(screen.getByText('Select an option - No answer provided')).toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()
  })

  it('clears the selection error when an action is selected and waits for Continue to navigate', async () => {
    renderActions()
    await user.click(screen.getByRole('button', { name: 'Continue' }))
    expect(screen.getByText('Select an option - No answer provided')).toBeInTheDocument()

    await user.click(screen.getByRole('radio', { name: 'Change user permissions' }))
    expect(screen.getByRole('radio', { name: 'Change user permissions' })).toBeChecked()
    expect(screen.queryByText('Select an option - No answer provided')).not.toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()

    await user.click(screen.getByRole('button', { name: 'Continue' }))
    expect(router.push).toHaveBeenCalledExactlyOnceWith(
      '/portal/organisations/2/manage-user-access/4/change-permissions',
    )
  })

  it.each([
    ['Change user permissions', '/portal/organisations/2/manage-user-access/4/change-permissions'],
    ['Deactivate user', '/portal/organisations/2/users/4/deactivate'],
    ['Remove user', '/portal/organisations/2/manage-user-access/4/remove'],
    ['Reactivate user', '/portal/organisations/2/users/4/reactivate'],
  ])('requests navigation to the selected %s action', async (action, href) => {
    renderActions(action === 'Reactivate user' ? 'Deactivated' : 'Active')

    await user.click(screen.getByRole('radio', { name: action }))
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(router.push).toHaveBeenCalledExactlyOnceWith(href)
  })

  // Navigation to the placeholder manage-details route remains deferred.
})
