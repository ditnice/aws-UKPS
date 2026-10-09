import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import ModifyUserMembershipRequestControls, {
  ModifyUserMembershipRequestControlsProps,
} from './ModifyUserMembershipRequestControls'

const mocks = vi.hoisted(() => ({
  approve: vi.fn(),
  reject: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  approve: mocks.approve,
  reject: mocks.reject,
}))
beforeEach(() => {
  mocks.approve.mockResolvedValue({
    response: { ok: true },
  })

  mocks.reject.mockResolvedValue({
    response: { ok: true },
  })
})

const renderComponent = (overrides?: Partial<ModifyUserMembershipRequestControlsProps>) => {
  const defaultProps: ModifyUserMembershipRequestControlsProps = {
    action: 'Approve',
    organisationId: 1,
    requestGuid: 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d',
    backLink: 'backLink',
    successLink: 'successLink',
  }
  const props = { ...defaultProps, ...overrides }
  render(<ModifyUserMembershipRequestControls {...props} />)
}

const getActionButton = () => {
  return screen.getByTestId('action-button')
}

const clickActionButton = async () => {
  await user.click(getActionButton())
}

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('ModifyUserMembershipRequestControls', () => {
  it('renders the approve action', () => {
    renderComponent({ action: 'Approve' })
    expect(getActionButton().textContent).toBe('Approve user')
  })

  it('renders the reject action', () => {
    renderComponent({ action: 'Reject' })
    expect(getActionButton().textContent).toBe('Reject user')
  })

  it('renders a cancel button that has the backlink as href', () => {
    const backLink = 'test/back/link'
    renderComponent({ backLink })
    expect(screen.getByTestId('cancel-button').getAttribute('href')).toBe(backLink)
  })

  it('sends the correct approve request for the approve action', async () => {
    const args = { organisationId: 14, requestGuid: 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d' }
    renderComponent({ ...args, action: 'Approve' })
    await clickActionButton()
    expect(mocks.approve).toHaveBeenCalledExactlyOnceWith({ path: args })
  })

  it('sends the correct reject request for the reject action', async () => {
    const args = { organisationId: 67, requestGuid: 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d' }
    renderComponent({ ...args, action: 'Reject' })
    await clickActionButton()
    expect(mocks.reject).toHaveBeenCalledExactlyOnceWith({ path: args })
  })

  it('pushes the success route on request success', async () => {
    mocks.approve.mockResolvedValue({ response: { ok: true } })
    const successLink = 'success/link'
    renderComponent({ action: 'Approve', successLink })
    await clickActionButton()
    await waitFor(() => {
      expect(router.push).toHaveBeenCalledExactlyOnceWith(successLink)
    })
  })

  it('shows an error on the request fail and does not push a new route', async () => {
    mocks.approve.mockResolvedValue({ response: { ok: false } })
    renderComponent()
    await clickActionButton()
    await waitFor(() => {
      expect(router.push).not.toHaveBeenCalled()
      expect(screen.getByTestId('action-error')).toBeInTheDocument()
    })
  })
})
