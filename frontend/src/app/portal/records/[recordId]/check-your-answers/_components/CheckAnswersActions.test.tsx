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
})
