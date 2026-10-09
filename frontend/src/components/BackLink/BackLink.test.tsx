import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { BackLink } from './BackLink'

describe('BackLink', () => {
  it('renders the default link text', () => {
    const { asFragment } = render(<BackLink href="/previous" />)

    expect(screen.getByRole('link', { name: 'Back' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies the href', () => {
    render(<BackLink href="/previous" />)

    expect(screen.getByRole('link').getAttribute('href')).toBe('/previous')
  })

  it('supports custom content', () => {
    render(<BackLink href="/components">Back to components</BackLink>)

    expect(screen.getByRole('link', { name: 'Back to components' })).toBeInTheDocument()
  })

  it('forwards native anchor attributes', () => {
    render(
      <BackLink aria-label="Go back to the previous step" href="/previous" id="previous-step" />,
    )

    const link = screen.getByRole('link', { name: 'Go back to the previous step' })
    expect(link.getAttribute('id')).toBe('previous-step')
  })

  it('preserves custom class names', () => {
    render(
      <BackLink className="additional-class" href="/previous">
        Back
      </BackLink>,
    )

    expect(screen.getByRole('link').classList.contains('additional-class')).toBe(true)
  })

  it('renders the inverse variant', () => {
    const { asFragment } = render(
      <BackLink href="/previous" variant="inverse">
        Back
      </BackLink>,
    )

    expect(screen.getByRole('link').className).toMatch(/back-link--inverse/)
    expect(asFragment()).toMatchSnapshot()
  })
})
