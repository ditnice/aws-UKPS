import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import ModifyUserMembershipRequestControls, {
  ModifyUserMembershipRequestControlsProps,
} from './ModifyUserMembershipRequestControls'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  approve: vi.fn(),
  reject: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mocks.push,
  }),
}))

vi.mock('@/client/generated', () => ({
  approve: mocks.approve,
  reject: mocks.reject,
}))

afterEach(cleanup)

beforeEach(() => {
  vi.clearAllMocks()

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
    userId: 2,
    backLink: 'backLink',
    successLink: 'successLink',
  }
  const props = { ...defaultProps, ...overrides }
  render(<ModifyUserMembershipRequestControls {...props} />)
}

const getActionButton = () => {
  return screen.getByTestId('action-button')
}

const clickActionButton = () => {
  fireEvent.click(getActionButton())
}

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

  it('sends the correct approve request for the approve action', () => {
    const args = { userId: 14, organisationId: 23 }
    renderComponent({ ...args, action: 'Approve' })
    clickActionButton()
    expect(mocks.approve).toHaveBeenCalledExactlyOnceWith({ path: args })
  })

  it('sends the correct reject request for the reject action', () => {
    const args = { userId: 67, organisationId: 3 }
    renderComponent({ ...args, action: 'Reject' })
    clickActionButton()
    expect(mocks.reject).toHaveBeenCalledExactlyOnceWith({ path: args })
  })

  it('pushes the success route on request success', async () => {
    mocks.approve.mockResolvedValue({ response: { ok: true } })
    const successLink = 'success/link'
    renderComponent({ action: 'Approve', successLink })
    clickActionButton()
    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledExactlyOnceWith(successLink)
    })
  })

  it('shows an error on the request fail and does not push a new route', async () => {
    mocks.approve.mockResolvedValue({ response: { ok: false } })
    renderComponent()
    clickActionButton()
    await waitFor(() => {
      expect(mocks.push).not.toHaveBeenCalled()
      expect(screen.getByTestId('action-error')).toBeDefined()
    })
  })
})
