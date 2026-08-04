import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { BackLink } from './BackLink'

afterEach(cleanup)

describe('BackLink', () => {
  it('renders the default link text', () => {
    const { asFragment } = render(<BackLink href="/previous" />)

    expect(screen.getByRole('link', { name: 'Back' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies the href', () => {
    const { asFragment } = render(<BackLink href="/previous" />)

    expect(screen.getByRole('link').getAttribute('href')).toBe('/previous')
    expect(asFragment()).toMatchSnapshot()
  })

  it('supports custom content', () => {
    const { asFragment } = render(<BackLink href="/components">Back to components</BackLink>)

    expect(screen.getByRole('link', { name: 'Back to components' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards native anchor attributes', () => {
    const { asFragment } = render(
      <BackLink aria-label="Go back to the previous step" href="/previous" id="previous-step" />,
    )

    const link = screen.getByRole('link', { name: 'Go back to the previous step' })
    expect(link.getAttribute('id')).toBe('previous-step')
    expect(asFragment()).toMatchSnapshot()
  })

  it('preserves custom class names', () => {
    const { asFragment } = render(
      <BackLink className="additional-class" href="/previous">
        Back
      </BackLink>,
    )

    expect(screen.getByRole('link').classList.contains('additional-class')).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })
})
