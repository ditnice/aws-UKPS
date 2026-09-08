import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import ReactivateUserControls, { ReactivateUserControlsProps } from './ReactivateUserControls'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  back: vi.fn(),
  reactivateMembership: vi.fn(),
  buildUserActionHref: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mocks.push,
    back: mocks.back,
  }),
}))

vi.mock('@/client/generated', () => ({
  reactivateMembership: mocks.reactivateMembership,
}))

vi.mock('../../../_lib/userActionAlert', () => ({
  buildUserActionHref: mocks.buildUserActionHref,
}))

afterEach(cleanup)

const mockHref = 'href'
beforeEach(() => {
  vi.clearAllMocks()

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

describe('DeactivateUserControls', () => {
  it('error is not rendered by default', () => {
    expect(getActionError()).toBeFalsy()
  })
  it('calls reactivate membership with the correct values', async () => {
    renderComponent()
    fireEvent.click(getActionButton())
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
    fireEvent.click(getActionButton())
    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledExactlyOnceWith(mockHref)
      expect(mocks.buildUserActionHref).toHaveBeenCalledWith(
        defaultProps.organisationId,
        'reactivated',
        defaultProps.userId,
      )
    })
  })
  it('shows an error state on error', async () => {
    mocks.reactivateMembership.mockReturnValue({ error: {} })

    renderComponent()
    fireEvent.click(getActionButton())
    await waitFor(() => {
      expect(getActionError()).toBeTruthy()
      expect(mocks.push).not.toHaveBeenCalled()
    })
  })

  it('call the router back function on cancel', async () => {
    renderComponent()
    fireEvent.click(getCancelButton())
    await waitFor(() => {
      expect(mocks.back).toHaveBeenCalledOnce()
    })
  })
})
