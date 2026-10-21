import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'

import { Details } from './Details'

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('Details', () => {
  it('renders closed by default with the given summary text and content', () => {
    const { asFragment } = render(<Details summary="Help with nationality">Some content</Details>)

    const summary = screen.getByText('Help with nationality')
    const details = summary.closest('details')!
    expect(details.open).toBe(false)

    expect(screen.getByText('Some content')).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders open when the open prop is set', () => {
    const { asFragment } = render(
      <Details open summary="Help with nationality">
        Some content
      </Details>,
    )

    const details = screen.getByText('Help with nationality').closest('details')!
    expect(details.open).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('toggles open when the summary is clicked', async () => {
    render(<Details summary="Help with nationality">Some content</Details>)

    const summary = screen.getByText('Help with nationality')
    await user.click(summary)

    expect(summary.closest('details')!.open).toBe(true)
  })

  it('closes again when the summary is clicked a second time', async () => {
    render(
      <Details open summary="Help with nationality">
        Some content
      </Details>,
    )

    const summary = screen.getByText('Help with nationality')
    await user.click(summary)

    expect(summary.closest('details')!.open).toBe(false)
  })

  it('forwards native attributes to the details element', () => {
    render(
      <Details id="nationality-details" summary="Help with nationality">
        Some content
      </Details>,
    )

    expect(document.getElementById('nationality-details')).not.toBeNull()
  })

  it('preserves custom class names', () => {
    render(
      <Details className="additional-class" summary="Help with nationality">
        Some content
      </Details>,
    )

    expect(screen.getByText('Help with nationality').closest('details')?.className).toContain(
      'additional-class',
    )
  })
})
