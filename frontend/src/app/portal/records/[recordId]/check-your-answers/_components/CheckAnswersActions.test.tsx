import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { CheckAnswersActions } from './CheckAnswersActions'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  refresh: vi.fn(),
  submitRecord: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mocks.push, refresh: mocks.refresh }),
}))

vi.mock('@/client/generated', async (importOriginal) => {
  const original = await importOriginal<typeof import('@/client/generated')>()
  return { ...original, submitRecord: mocks.submitRecord }
})

const props = {
  recordId: 5,
  formVersion: 'medicine-1',
  revisionHash: 'hash-1',
  organisationId: 7,
  canSubmit: true,
}

describe('CheckAnswersActions', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mocks.submitRecord.mockResolvedValue({ response: { ok: true, status: 204 } })
  })

  it('renders the submit button and a link back to the records', () => {
    render(<CheckAnswersActions {...props} />)

    expect(
      screen.getByRole('heading', { level: 2, name: 'Now submit your record for QA review' }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Submit record for review' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Return to my records' })).toHaveAttribute(
      'href',
      '/portal/organisations/7/records?action=draft-saved',
    )
  })

  it('submits the record and goes to the records list when Submit is clicked', async () => {
    render(<CheckAnswersActions {...props} />)

    await userEvent.click(screen.getByRole('button', { name: 'Submit record for review' }))

    await waitFor(() =>
      expect(mocks.push).toHaveBeenCalledExactlyOnceWith(
        '/portal/organisations/7/records?action=submitted',
      ),
    )
    expect(mocks.submitRecord).toHaveBeenCalledExactlyOnceWith({
      path: { recordId: 5 },
      body: { formVersion: 'medicine-1', revisionHash: 'hash-1' },
    })
  })

  it('disables Submit while the submission is in progress', async () => {
    let finish: (value: unknown) => void = () => {}
    mocks.submitRecord.mockReturnValueOnce(
      new Promise((resolve) => {
        finish = resolve
      }),
    )
    render(<CheckAnswersActions {...props} />)
    const submitButton = screen.getByRole('button', { name: 'Submit record for review' })

    await userEvent.click(submitButton)
    expect(submitButton).toBeDisabled()

    finish({ error: {}, response: { ok: false, status: 500 } })
    await waitFor(() => expect(submitButton).toBeEnabled())
  })

  it('shows the API message and a reload link when the record has changed', async () => {
    mocks.submitRecord.mockResolvedValueOnce({
      error: {
        code: 'record_changed',
        detail: 'This record has changed since check your answers was loaded.',
      },
      response: { ok: false, status: 409 },
    })
    render(<CheckAnswersActions {...props} />)

    await userEvent.click(screen.getByRole('button', { name: 'Submit record for review' }))

    expect(
      await screen.findByText('This record has changed since check your answers was loaded.'),
    ).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Reload this page' })).toHaveAttribute(
      'href',
      '/portal/records/5/check-your-answers',
    )
    expect(mocks.push).not.toHaveBeenCalled()
  })

  it('shows a general message when the API gives no reason for the conflict', async () => {
    mocks.submitRecord.mockResolvedValueOnce({
      error: undefined,
      response: { ok: false, status: 409 },
    })
    render(<CheckAnswersActions {...props} />)

    await userEvent.click(screen.getByRole('button', { name: 'Submit record for review' }))

    expect(
      await screen.findByText('This record has changed. Reload the page and try again.'),
    ).toBeInTheDocument()
    expect(mocks.push).not.toHaveBeenCalled()
  })

  it('tells the user to fix their answers and refreshes the page when the answers are invalid', async () => {
    mocks.submitRecord.mockResolvedValueOnce({
      error: {},
      response: { ok: false, status: 422 },
    })
    render(<CheckAnswersActions {...props} />)

    await userEvent.click(screen.getByRole('button', { name: 'Submit record for review' }))

    expect(await screen.findByText(/There are problems with your answers/)).toBeInTheDocument()
    expect(mocks.refresh).toHaveBeenCalledOnce()
  })

  it('shows a try again message and does not refresh the page when the submission fails', async () => {
    mocks.submitRecord.mockResolvedValueOnce({
      error: {},
      response: { ok: false, status: 500 },
    })
    render(<CheckAnswersActions {...props} />)

    await userEvent.click(screen.getByRole('button', { name: 'Submit record for review' }))

    expect(
      await screen.findByText(
        'There was a problem submitting your record. Please try again later.',
      ),
    ).toBeInTheDocument()
    expect(mocks.refresh).not.toHaveBeenCalled()
  })

  it('hides the Submit record button when errors are present', async () => {
    render(<CheckAnswersActions {...props} canSubmit={false} />)

    expect(
      screen.queryByRole('button', { name: 'Submit record for review' }),
    ).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Return to my records' })).toBeInTheDocument()
  })
})
