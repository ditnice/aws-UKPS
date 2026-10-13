import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import RemoveUserControls, { RemoveUserControlsProps } from './RemoveUserControls'

import type { ComponentPropsWithoutRef } from 'react'

const mocks = vi.hoisted(() => ({
  removeUser: vi.fn(),
  buildUserActionHref: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

// Use a plain anchor so Next's own navigation handler cannot mask a missing Cancel guard.
vi.mock('next/link', () => ({
  default: ({ children, ...props }: ComponentPropsWithoutRef<'a'>) => <a {...props}>{children}</a>,
}))

vi.mock('@/client/generated', () => ({
  removeUser: mocks.removeUser,
}))

vi.mock('../../../_lib/userActionAlert', () => ({
  buildUserActionHref: mocks.buildUserActionHref,
}))

let user: ReturnType<typeof userEvent.setup>
const mockHref = 'href'
beforeEach(() => {
  user = userEvent.setup()

  mocks.removeUser.mockReturnValue({})
  mocks.buildUserActionHref.mockReturnValue(mockHref)
})

const defaultProps: RemoveUserControlsProps = {
  organisationId: 1,
  userId: 3,
}
const renderComponent = () => {
  render(<RemoveUserControls {...defaultProps} />)
}

const getActionButton = () => screen.getByRole('button', { name: 'Remove user' })
const getCancelLink = () => screen.getByRole('link', { name: 'Cancel' })
const getActionError = () => screen.queryByRole('alert')

describe('RemoveUserControls', () => {
  it('does not render the error by default', () => {
    renderComponent()

    expect(getActionError()).toBeFalsy()
  })

  it('calls remove user with the correct user id', async () => {
    renderComponent()
    await user.click(getActionButton())
    await waitFor(() => {
      expect(mocks.removeUser).toHaveBeenCalledExactlyOnceWith({
        path: { userId: defaultProps.userId },
      })
    })
  })

  it('routes to the organisation page on success', async () => {
    renderComponent()
    await user.click(getActionButton())
    await waitFor(() => {
      expect(router.push).toHaveBeenCalledExactlyOnceWith(mockHref)
      expect(mocks.buildUserActionHref).toHaveBeenCalledWith(defaultProps.organisationId, {
        action: 'removed',
        userId: defaultProps.userId,
      })
    })
  })

  it('shows an error state on error', async () => {
    mocks.removeUser.mockReturnValue({ error: {} })

    renderComponent()
    await user.click(getActionButton())
    await waitFor(() => {
      expect(getActionError()).toBeTruthy()
      expect(router.push).not.toHaveBeenCalled()
    })
  })

  it('links cancel back to the manage user access page', () => {
    renderComponent()

    expect(getCancelLink().getAttribute('href')).toBe(
      `/portal/organisations/${defaultProps.organisationId}/manage-user-access/${defaultProps.userId}`,
    )
  })

  it('blocks Cancel while removal is pending and restores it after failure', async () => {
    let resolveRequest!: (value: { error: object }) => void
    mocks.removeUser.mockReturnValue(
      new Promise<{ error: object }>((resolve) => {
        resolveRequest = resolve
      }),
    )
    renderComponent()

    await user.click(getActionButton())

    expect(getCancelLink().getAttribute('aria-disabled')).toBe('true')
    const pendingClick = new MouseEvent('click', { bubbles: true, cancelable: true })
    fireEvent(getCancelLink(), pendingClick)
    expect(pendingClick.defaultPrevented).toBe(true)

    resolveRequest({ error: {} })
    await waitFor(() => {
      expect(getCancelLink().getAttribute('aria-disabled')).toBe('false')
    })
    expect(getCancelLink().getAttribute('href')).toBe(
      `/portal/organisations/${defaultProps.organisationId}/manage-user-access/${defaultProps.userId}`,
    )
    expect(getActionError()).toBeTruthy()
  })
})
