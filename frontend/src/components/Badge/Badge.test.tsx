import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { Badge } from './Badge'

import type { BadgeColour } from './Badge'

const colours: BadgeColour[] = ['blue', 'purple', 'bright-purple', 'red', 'green', 'black', 'grey']

describe('Badge', () => {
  it('renders the text in a span with the default blue colour', () => {
    const { asFragment } = render(<Badge>Recently added</Badge>)

    const badge = screen.getByText('Recently added')
    expect(badge.tagName).toBe('SPAN')
    expect(badge).toHaveAttribute('data-component', 'badge')
    expect(badge.className).toContain('badge')
    expect(badge.className).toContain('blue')
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(colours)('renders the %s colour variant', (colour) => {
    const { asFragment } = render(<Badge colour={colour}>Status</Badge>)

    expect(screen.getByText('Status').className).toContain(colour)
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies the large modifier', () => {
    const { asFragment } = render(
      <Badge colour="red" large>
        Urgent
      </Badge>,
    )

    expect(screen.getByText('Urgent').className).toContain('large')
    expect(asFragment()).toMatchSnapshot()
  })

  it('does not apply the large modifier by default', () => {
    render(<Badge>Urgent</Badge>)

    expect(screen.getByText('Urgent').className).not.toContain('large')
  })

  it('renders a visually hidden label before the text', () => {
    render(
      <Badge colour="red" label="Case priority:">
        Urgent
      </Badge>,
    )

    const label = screen.getByText('Case priority:')
    expect(label).toHaveClass('visually-hidden')
    expect(label.parentElement).toHaveTextContent('Case priority: Urgent')
  })

  it('forwards native attributes to the span', () => {
    render(
      <Badge data-testid="urgent-badge" id="urgent">
        Urgent
      </Badge>,
    )

    expect(screen.getByTestId('urgent-badge')).toHaveAttribute('id', 'urgent')
  })

  it('preserves custom class names', () => {
    render(<Badge className="additional-class">Urgent</Badge>)

    expect(screen.getByText('Urgent')).toHaveClass('additional-class')
  })
})
