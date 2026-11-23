import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { archiveRecord } from '@/client/generated'
import { router } from '@/test-utils/nextNavigation'

import { loadReasonAnswers, saveReasonAnswers } from '../_lib/reasonAnswerStorage'
import { statusChangeErrorMessages } from '../_lib/statusChangeActions'

import { ConfirmStatusChangeControls } from './ConfirmStatusChangeControls'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  archiveRecord: vi.fn(),
  putRecordOnHold: vi.fn(),
  resumeRecord: vi.fn(),
}))

type SubmitResult = Awaited<ReturnType<typeof archiveRecord>>

const mockSubmit = (result: { status: number; error?: unknown }) =>
  vi.mocked(archiveRecord).mockResolvedValue({
    error: result.error,
    response: new Response(null, { status: result.status }),
  } as SubmitResult)

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
  mockSubmit({ status: 200 })
  saveReasonAnswers(42, 'Archive', { reason: 'Other', note: 'Rights sold' })
})

afterEach(() => {
  sessionStorage.clear()
})

const renderConfirm = () =>
  render(
    <ConfirmStatusChangeControls
      organisationId={7}
      recordId={42}
      recordType="Medicine"
      action="Archive"
    />,
  )

const confirmButton = () => screen.getByRole('button', { name: 'Confirm' })

describe('ConfirmStatusChangeControls', () => {
  it('makes the change with the saved answers and returns to the record with an alert', async () => {
    renderConfirm()

    await user.click(confirmButton())

    expect(archiveRecord).toHaveBeenCalledExactlyOnceWith({
      path: { recordId: 42 },
      query: { organisationId: 7 },
      body: { reason: 'Other', note: 'Rights sold' },
    })
    await waitFor(() => {
      expect(router.push).toHaveBeenCalledExactlyOnceWith(
        '/portal/organisations/7/records/42?recordType=Medicine&action=archived',
      )
    })
    expect(loadReasonAnswers(42, 'Archive')).toBeUndefined()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    // Clearing the saved answers must not send the user back to the reason page instead.
    expect(router.replace).not.toHaveBeenCalled()
  })

  it('sends the user to the reason page when there are no saved answers', async () => {
    sessionStorage.clear()

    renderConfirm()

    await waitFor(() => {
      expect(router.replace).toHaveBeenCalledExactlyOnceWith(
        '/portal/organisations/7/records/42/change-record-status/archive?recordType=Medicine',
      )
    })
    expect(confirmButton()).toBeDisabled()
  })

  it.each([
    ['an invalid reason', 400, { errors: { Reason: ['Invalid'] } }, 'invalidReason'],
    ['a missing note', 400, { errors: { Note: ['Required'] } }, 'noteRequired'],
    [
      'a change the record no longer permits',
      409,
      { code: 'status_change_not_permitted' },
      'notPermitted',
    ],
    ['a concurrent update', 409, { code: 'record_changed' }, 'recordChanged'],
    ['an unexpected failure', 500, {}, 'failed'],
  ] as const)('explains %s and keeps the saved answers', async (_, status, error, message) => {
    mockSubmit({ status, error })
    renderConfirm()

    await user.click(confirmButton())

    expect(await screen.findByRole('alert')).toHaveTextContent(statusChangeErrorMessages[message])
    expect(router.push).not.toHaveBeenCalled()
    expect(confirmButton()).toBeEnabled()
    expect(loadReasonAnswers(42, 'Archive')).toBeDefined()
  })

  it('shows an error when the request cannot be sent', async () => {
    vi.mocked(archiveRecord).mockRejectedValue(new Error('offline'))
    renderConfirm()

    await user.click(confirmButton())

    expect(await screen.findByRole('alert')).toHaveTextContent(statusChangeErrorMessages.failed)
  })

  it('leaves an organisation mismatch to the central organisation handling', async () => {
    mockSubmit({ status: 409, error: { code: 'SelectedOrganisationMismatch', organisationId: 7 } })
    renderConfirm()

    await user.click(confirmButton())

    await waitFor(() => {
      expect(confirmButton()).toBeEnabled()
    })
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()
  })

  it('clears a previous error when confirming again', async () => {
    mockSubmit({ status: 409, error: { code: 'record_changed' } })
    renderConfirm()
    await user.click(confirmButton())
    await screen.findByRole('alert')

    mockSubmit({ status: 200 })
    await user.click(confirmButton())

    await waitFor(() => {
      expect(router.push).toHaveBeenCalledOnce()
    })
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('disables Confirm while the request is pending', async () => {
    let resolveRequest!: (value: SubmitResult) => void
    vi.mocked(archiveRecord).mockReturnValue(
      new Promise<SubmitResult>((resolve) => {
        resolveRequest = resolve
      }) as ReturnType<typeof archiveRecord>,
    )
    renderConfirm()

    await user.click(confirmButton())

    expect(confirmButton()).toBeDisabled()
    resolveRequest({ error: {}, response: new Response(null, { status: 500 }) } as SubmitResult)
    await waitFor(() => {
      expect(confirmButton()).toBeEnabled()
    })
  })

  it('links Cancel back to the record and forgets the saved answers', async () => {
    renderConfirm()

    const cancel = screen.getByRole('link', { name: 'Cancel' })
    expect(cancel).toHaveAttribute('href', '/portal/organisations/7/records/42?recordType=Medicine')
    await user.click(cancel)

    expect(loadReasonAnswers(42, 'Archive')).toBeUndefined()
    expect(router.replace).not.toHaveBeenCalled()
  })
})
