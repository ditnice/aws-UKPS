import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import { CheckAnswersActions } from './CheckAnswersActions'

const returnHref = '/portal/organisations/1/records'

describe('CheckAnswersActions', () => {
  it('renders the submit button and a link back to the records', () => {
    render(<CheckAnswersActions onSubmit={vi.fn()} returnHref={returnHref} />)

    expect(
      screen.getByRole('heading', { level: 2, name: 'Now submit your record for QA review' }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Submit record for review' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Return to my records' })).toHaveAttribute(
      'href',
      returnHref,
    )
  })

  it('submits when Submit is clicked', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined)
    render(<CheckAnswersActions onSubmit={onSubmit} returnHref={returnHref} />)

    await userEvent.click(screen.getByRole('button', { name: 'Submit record for review' }))

    expect(onSubmit).toHaveBeenCalledOnce()
  })

  it('disables Submit while the submission is in progress', async () => {
    let finish: () => void = () => {}
    const onSubmit = vi.fn(
      () =>
        new Promise<void>((resolve) => {
          finish = resolve
        }),
    )
    render(<CheckAnswersActions onSubmit={onSubmit} returnHref={returnHref} />)
    const submitButton = screen.getByRole('button', { name: 'Submit record for review' })

    await userEvent.click(submitButton)
    expect(submitButton).toBeDisabled()

    finish()
    await waitFor(() => expect(submitButton).toBeEnabled())
  })
})
