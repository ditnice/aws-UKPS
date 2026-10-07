import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Details } from './Details'

afterEach(cleanup)

describe('Details', () => {
  it('renders closed by default with the given summary text and content', () => {
    const { asFragment } = render(<Details summary="Help with nationality">Some content</Details>)

    const summary = screen.getByText('Help with nationality')
    const details = summary.closest('details')!
    expect(details.open).toBe(false)

    expect(screen.getByText('Some content')).toBeDefined()
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

  it('toggles open when the summary is clicked', () => {
    const { asFragment } = render(<Details summary="Help with nationality">Some content</Details>)

    const summary = screen.getByText('Help with nationality')
    fireEvent.click(summary)

    expect(summary.closest('details')!.open).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('closes again when the summary is clicked a second time', () => {
    const { asFragment } = render(
      <Details open summary="Help with nationality">
        Some content
      </Details>,
    )

    const summary = screen.getByText('Help with nationality')
    fireEvent.click(summary)

    expect(summary.closest('details')!.open).toBe(false)
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards native attributes to the details element', () => {
    const { asFragment } = render(
      <Details id="nationality-details" summary="Help with nationality">
        Some content
      </Details>,
    )

    expect(document.getElementById('nationality-details')).not.toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('preserves custom class names', () => {
    const { asFragment } = render(
      <Details className="additional-class" summary="Help with nationality">
        Some content
      </Details>,
    )

    expect(screen.getByText('Help with nationality').closest('details')?.className).toContain(
      'additional-class',
    )
    expect(asFragment()).toMatchSnapshot()
  })
})
