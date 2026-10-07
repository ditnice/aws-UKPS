import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import ManageUserAccessActions from './ManageUserAccessActions'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

function renderActions(currentUserRole = 'Super') {
  render(
    <ManageUserAccessActions
      organisationId={2}
      selectedUserId={4}
      currentUserRole={currentUserRole}
    />,
  )
}

describe('ManageUserAccessActions', () => {
  it('offers the available actions without selecting one by default', () => {
    renderActions()

    for (const name of [
      'Change user permissions',
      'Deactivate user',
      /Manage user details and sign in method/,
    ]) {
      expect(screen.getByRole('radio', { name })).not.toBeChecked()
    }
  })

  it('offers the remove action to a Super user', () => {
    renderActions('Super')
    expect(
      screen.getByRole('radio', { name: 'Remove user - not implemented yet' }),
    ).toBeInTheDocument()
  })

  it.each(['Champion', 'Standard'])('does not offer the remove action to a %s user', (role) => {
    renderActions(role)
    expect(
      screen.queryByRole('radio', { name: 'Remove user - not implemented yet' }),
    ).not.toBeInTheDocument()
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
  ])('requests navigation to the selected %s action', async (action, href) => {
    renderActions()

    await user.click(screen.getByRole('radio', { name: action }))
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(router.push).toHaveBeenCalledExactlyOnceWith(href)
  })

  // Navigation to the placeholder remove/manage-details routes remains deferred.
})
