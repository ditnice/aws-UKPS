import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { BackLink } from './BackLink'

afterEach(cleanup)

describe('BackLink', () => {
  it('renders the default link text', () => {
    render(<BackLink href="/previous" />)

    expect(screen.getByRole('link', { name: 'Back' })).toBeDefined()
  })

  it('applies the href', () => {
    render(<BackLink href="/previous" />)

    expect(screen.getByRole('link').getAttribute('href')).toBe('/previous')
  })

  it('supports custom content', () => {
    render(<BackLink href="/components">Back to components</BackLink>)

    expect(screen.getByRole('link', { name: 'Back to components' })).toBeDefined()
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
})
