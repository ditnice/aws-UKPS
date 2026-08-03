import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Details } from './Details'

afterEach(cleanup)

describe('Details', () => {
  it('renders closed by default with the given summary text and content', () => {
    render(<Details summary="Help with nationality">Some content</Details>)

    const summary = screen.getByText('Help with nationality')
    const details = summary.closest('details')!
    expect(details.open).toBe(false)

    expect(screen.getByText('Some content')).toBeDefined()
  })

  it('renders open when the open prop is set', () => {
    render(
      <Details open summary="Help with nationality">
        Some content
      </Details>,
    )

    const details = screen.getByText('Help with nationality').closest('details')!
    expect(details.open).toBe(true)
  })

  it('toggles open when the summary is clicked', () => {
    render(<Details summary="Help with nationality">Some content</Details>)

    const summary = screen.getByText('Help with nationality')
    fireEvent.click(summary)

    expect(summary.closest('details')!.open).toBe(true)
  })

  it('closes again when the summary is clicked a second time', () => {
    render(
      <Details open summary="Help with nationality">
        Some content
      </Details>,
    )

    const summary = screen.getByText('Help with nationality')
    fireEvent.click(summary)

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
})
