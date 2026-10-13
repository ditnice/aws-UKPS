import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import ReactivateUserControls, { ReactivateUserControlsProps } from './ReactivateUserControls'

const mocks = vi.hoisted(() => ({
  reactivateMembership: vi.fn(),
  buildUserActionHref: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  reactivateMembership: mocks.reactivateMembership,
}))

vi.mock('../../../_lib/userActionAlert', () => ({
  buildUserActionHref: mocks.buildUserActionHref,
}))
const mockHref = 'href'
beforeEach(() => {
  mocks.reactivateMembership.mockReturnValue({})
  mocks.buildUserActionHref.mockReturnValue(mockHref)
})

const defaultProps: ReactivateUserControlsProps = {
  organisationId: 1,
  membershipId: 2,
  userId: 3,
}
const renderComponent = () => {
  render(<ReactivateUserControls {...defaultProps} />)
}

const getActionButton = () => screen.getByTestId('action-button')
const getCancelButton = () => screen.getByTestId('cancel-button')
const getActionError = () => screen.queryByTestId('action-error')

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('ReactivateUserControls', () => {
  it('error is not rendered by default', () => {
    expect(getActionError()).toBeFalsy()
  })
  it('calls reactivate membership with the correct values', async () => {
    renderComponent()
    await user.click(getActionButton())
    await waitFor(() => {
      expect(mocks.reactivateMembership).toHaveBeenCalledExactlyOnceWith({
        path: {
          organisationId: defaultProps.organisationId,
          membershipId: defaultProps.membershipId,
        },
      })
    })
  })
  it('routes to the organisation page on success', async () => {
    renderComponent()
    await user.click(getActionButton())
    await waitFor(() => {
      expect(router.push).toHaveBeenCalledExactlyOnceWith(mockHref)
      expect(mocks.buildUserActionHref).toHaveBeenCalledWith(defaultProps.organisationId, {
        action: 'reactivated',
        userId: defaultProps.userId,
      })
    })
  })
  it('shows an error state on error', async () => {
    mocks.reactivateMembership.mockReturnValue({ error: {} })

    renderComponent()
    await user.click(getActionButton())
    await waitFor(() => {
      expect(getActionError()).toBeTruthy()
      expect(router.push).not.toHaveBeenCalled()
    })
  })

  it('call the router back function on cancel', async () => {
    renderComponent()
    await user.click(getCancelButton())
    await waitFor(() => {
      expect(router.back).toHaveBeenCalledOnce()
    })
  })
})
