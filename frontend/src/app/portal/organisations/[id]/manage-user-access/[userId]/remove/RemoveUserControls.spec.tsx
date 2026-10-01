import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import RemoveUserControls, { RemoveUserControlsProps } from './RemoveUserControls'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  removeUser: vi.fn(),
  buildUserActionHref: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mocks.push,
  }),
}))

vi.mock('@/client/generated', () => ({
  removeUser: mocks.removeUser,
}))

vi.mock('../../../_lib/userActionAlert', () => ({
  buildUserActionHref: mocks.buildUserActionHref,
}))

afterEach(cleanup)

const mockHref = 'href'
beforeEach(() => {
  vi.clearAllMocks()

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

const getActionButton = () => screen.getByTestId('action-button')
const getCancelLink = () => screen.getByRole('link', { name: 'Cancel' })
const getActionError = () => screen.queryByTestId('action-error')

describe('RemoveUserControls', () => {
  it('does not render the error by default', () => {
    renderComponent()

    expect(getActionError()).toBeFalsy()
  })

  it('calls remove user with the correct user id', async () => {
    renderComponent()
    fireEvent.click(getActionButton())
    await waitFor(() => {
      expect(mocks.removeUser).toHaveBeenCalledExactlyOnceWith({
        path: { userId: defaultProps.userId },
      })
    })
  })

  it('routes to the organisation page on success', async () => {
    renderComponent()
    fireEvent.click(getActionButton())
    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledExactlyOnceWith(mockHref)
      expect(mocks.buildUserActionHref).toHaveBeenCalledWith(defaultProps.organisationId, {
        action: 'removed',
        userId: defaultProps.userId,
      })
    })
  })

  it('shows an error state on error', async () => {
    mocks.removeUser.mockReturnValue({ error: {} })

    renderComponent()
    fireEvent.click(getActionButton())
    await waitFor(() => {
      expect(getActionError()).toBeTruthy()
      expect(mocks.push).not.toHaveBeenCalled()
    })
  })

  it('links cancel back to the manage user access page', () => {
    renderComponent()

    expect(getCancelLink().getAttribute('href')).toBe(
      `/portal/organisations/${defaultProps.organisationId}/manage-user-access/${defaultProps.userId}`,
    )
  })
})
