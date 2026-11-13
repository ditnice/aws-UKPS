import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { CreateRecordPageHeader } from './CreateRecordPageHeader'

const backHref = '/portal/organisations/1/records'
const exitWarningHeading = 'If you continue, this record will not be saved'

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

const getBackLink = () => screen.getByRole('link', { name: 'Back' })
const queryExitWarning = () => screen.queryByRole('heading', { name: exitWarningHeading })

// Records whether the next click on the page was default-prevented once handlers have run.
const recordClicks = () => {
  const onClick = vi.fn((event: Event) => event.defaultPrevented)
  document.addEventListener('click', onClick, { once: true })
  return onClick
}

describe('CreateRecordPageHeader', () => {
  it('links Back to the given destination without showing the exit warning', () => {
    render(<CreateRecordPageHeader backHref={backHref} />)

    expect(getBackLink()).toHaveAttribute('href', backHref)
    expect(queryExitWarning()).not.toBeInTheDocument()
  })

  it('shows the exit warning instead of leaving the page when Back is first selected', async () => {
    render(<CreateRecordPageHeader backHref={backHref} />)
    const onClick = recordClicks()

    await user.click(getBackLink())

    expect(onClick).toHaveReturnedWith(true)
    expect(queryExitWarning()).toBeInTheDocument()
  })

  it('follows the link when Back is selected while the exit warning is showing', async () => {
    render(<CreateRecordPageHeader backHref={backHref} />)
    await user.click(getBackLink())
    const onClick = recordClicks()

    await user.click(getBackLink())

    expect(onClick).toHaveReturnedWith(false)
    expect(getBackLink()).toHaveAttribute('href', backHref)
  })
})
