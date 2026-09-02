import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { changeUserPermissionsAction } from '../_actions/changeUserPermissions'

import { ChangePermissionsForm } from './ChangePermissionsForm'

vi.mock('../_actions/changeUserPermissions', () => ({
  changeUserPermissionsAction: vi.fn(),
}))

const push = vi.hoisted(() => vi.fn())
const back = vi.hoisted(() => vi.fn())
vi.mock('next/navigation', () => ({ useRouter: () => ({ push, back }) }))

const props = {
  organisationId: 2,
  userId: 4,
  membershipId: 9,
} as const

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(changeUserPermissionsAction).mockResolvedValue({ status: 'success' })
})

afterEach(cleanup)

describe('ChangePermissionsForm', () => {
  it('offers to promote a standard user', () => {
    render(<ChangePermissionsForm {...props} currentRole="Standard" />)

    expect(screen.getByRole('button', { name: 'Make champion user' })).toBeDefined()
  })

  it('offers to demote a champion user', () => {
    render(<ChangePermissionsForm {...props} currentRole="Champion" />)

    expect(screen.getByRole('button', { name: 'Make standard user' })).toBeDefined()
  })

  it('promotes a standard user to champion', async () => {
    render(<ChangePermissionsForm {...props} currentRole="Standard" />)

    fireEvent.click(screen.getByRole('button', { name: 'Make champion user' }))

    await waitFor(() => {
      expect(changeUserPermissionsAction).toHaveBeenCalledWith(2, 4, 9, 'Champion')
    })
  })

  it('demotes a champion user to standard', async () => {
    render(<ChangePermissionsForm {...props} currentRole="Champion" />)

    fireEvent.click(screen.getByRole('button', { name: 'Make standard user' }))

    await waitFor(() => {
      expect(changeUserPermissionsAction).toHaveBeenCalledWith(2, 4, 9, 'Standard')
    })
  })

  it('returns to the main manage-organisation page with the user id once the role has changed', async () => {
    render(<ChangePermissionsForm {...props} currentRole="Standard" />)

    fireEvent.click(screen.getByRole('button', { name: 'Make champion user' }))

    await waitFor(() => {
      expect(push).toHaveBeenCalledWith(
        '/portal/organisations/2?action=permissions-updated&userId=4',
      )
    })
  })

  it('shows an error and stays on the page when the change fails', async () => {
    vi.mocked(changeUserPermissionsAction).mockResolvedValue({
      status: 'error',
      message: 'Something went wrong.',
    })

    render(<ChangePermissionsForm {...props} currentRole="Standard" />)

    fireEvent.click(screen.getByRole('button', { name: 'Make champion user' }))

    await waitFor(() => {
      expect(screen.getByRole('alert').textContent).toBe('Something went wrong.')
    })
    expect(push).not.toHaveBeenCalled()
    expect(screen.getByRole('button', { name: 'Make champion user' })).toBeDefined()
  })

  it('goes back when cancelled', () => {
    render(<ChangePermissionsForm {...props} currentRole="Standard" />)

    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(back).toHaveBeenCalled()
    expect(changeUserPermissionsAction).not.toHaveBeenCalled()
  })
})
