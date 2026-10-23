import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import { DiscardRecordControls, DiscardRecordControlsProps } from './DiscardRecordControls'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>
beforeEach(() => {
  user = userEvent.setup()
})

const defaultProps: DiscardRecordControlsProps = {
  organisationId: 7,
  recordId: 42,
}

const renderComponent = () => {
  render(<DiscardRecordControls {...defaultProps} />)
}

const getConfirmButton = () => screen.getByRole('button', { name: 'Confirm' })
const getCancelButton = () => screen.getByRole('button', { name: 'Cancel' })
const getActionError = () => screen.queryByRole('alert')

// TODO: cover the request, error and pending states once the discard endpoint is available.
describe('DiscardRecordControls', () => {
  it('does not render the error by default', () => {
    renderComponent()

    expect(getActionError()).toBeFalsy()
  })

  it('routes to the records list on success', async () => {
    renderComponent()
    await user.click(getConfirmButton())

    await waitFor(() => {
      expect(router.push).toHaveBeenCalledExactlyOnceWith('/portal/organisations/7/records')
    })
    expect(getActionError()).toBeFalsy()
  })

  it('calls the router back function on cancel', async () => {
    renderComponent()
    await user.click(getCancelButton())

    expect(router.back).toHaveBeenCalledOnce()
    expect(router.push).not.toHaveBeenCalled()
  })
})
