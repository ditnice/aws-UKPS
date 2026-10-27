import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import { DiscardRecordControls, DiscardRecordControlsProps } from './DiscardRecordControls'

const mocks = vi.hoisted(() => ({
  discardRecord: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  discardRecord: mocks.discardRecord,
}))

let user: ReturnType<typeof userEvent.setup>
beforeEach(() => {
  user = userEvent.setup()

  mocks.discardRecord.mockReturnValue({})
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

describe('DiscardRecordControls', () => {
  it('does not render the error by default', () => {
    renderComponent()

    expect(getActionError()).toBeFalsy()
  })

  it('calls discard record with the correct record id', async () => {
    renderComponent()
    await user.click(getConfirmButton())

    await waitFor(() => {
      expect(mocks.discardRecord).toHaveBeenCalledExactlyOnceWith({
        path: { recordId: defaultProps.recordId },
      })
    })
  })

  it('routes to the records list with the discarded confirmation on success', async () => {
    renderComponent()
    await user.click(getConfirmButton())

    await waitFor(() => {
      expect(router.push).toHaveBeenCalledExactlyOnceWith(
        '/portal/organisations/7/records?action=discarded',
      )
    })
    expect(getActionError()).toBeFalsy()
  })

  it('shows an error state on error', async () => {
    mocks.discardRecord.mockReturnValue({ error: {} })

    renderComponent()
    await user.click(getConfirmButton())

    await waitFor(() => {
      expect(getActionError()).toBeTruthy()
    })
    expect(router.push).not.toHaveBeenCalled()
  })

  it('disables both buttons while the request is pending', async () => {
    let resolveRequest!: (value: { error: object }) => void
    mocks.discardRecord.mockReturnValue(
      new Promise<{ error: object }>((resolve) => {
        resolveRequest = resolve
      }),
    )
    renderComponent()

    await user.click(getConfirmButton())

    expect(getConfirmButton()).toBeDisabled()
    expect(getCancelButton()).toBeDisabled()

    resolveRequest({ error: {} })
    await waitFor(() => {
      expect(getConfirmButton()).not.toBeDisabled()
    })
    expect(getCancelButton()).not.toBeDisabled()
  })

  it('calls the router back function on cancel', async () => {
    renderComponent()
    await user.click(getCancelButton())

    expect(router.back).toHaveBeenCalledOnce()
    expect(router.push).not.toHaveBeenCalled()
  })
})
