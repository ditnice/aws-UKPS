import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { Tag } from './Tag'

import type { TagColour } from './Tag'

const colours: { colour: TagColour; backgroundColor: string; color: string }[] = [
  { colour: 'grey', backgroundColor: '#cecece', color: '#0b0c0c' },
  { colour: 'green', backgroundColor: '#cfe4dc', color: '#083d29' },
  { colour: 'teal', backgroundColor: '#d0e6e7', color: '#0b4144' },
  { colour: 'blue', backgroundColor: '#d2e2f1', color: '#0f385c' },
  { colour: 'purple', backgroundColor: '#ddd6ec', color: '#2a1950' },
  { colour: 'magenta', backgroundColor: '#f4d7e5', color: '#651b3e' },
  { colour: 'red', backgroundColor: '#f4d7d7', color: '#651b1b' },
  { colour: 'orange', backgroundColor: '#fde4d7', color: '#7a3c1c' },
  { colour: 'yellow', backgroundColor: '#ffee80', color: '#7a3c1c' },
]

describe('Tag', () => {
  it.each(colours)('renders the $colour colour variant', ({ colour, backgroundColor, color }) => {
    const { asFragment } = render(<Tag colour={colour}>Status</Tag>)

    expect(screen.getByText('Status')).toHaveStyle({ backgroundColor, color })
    expect(asFragment()).toMatchSnapshot()
  })

  it('wraps the design-system tag when a className is supplied', () => {
    const { container } = render(
      <Tag className="additional-class" colour="green">
        Active
      </Tag>,
    )

    expect(container.firstElementChild?.classList.contains('additional-class')).toBe(true)
  })

  it('merges consumer styles with the colour styles', () => {
    render(
      <Tag colour="blue" style={{ border: '1px solid red' }}>
        Draft
      </Tag>,
    )

    const tag = screen.getByText('Draft')
    expect(tag.style.border).toBe('1px solid red')
    expect(tag.style.backgroundColor).toBe('rgb(210, 226, 241)')
  })

  it('passes through design-system modifier props and HTML attributes', () => {
    const { asFragment } = render(
      <Tag flush impact outline data-testid="priority-tag">
        High priority
      </Tag>,
    )

    expect(screen.getByTestId('priority-tag')).toHaveClass(
      'tag--flush',
      'tag--impact',
      'tag--outline',
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a remove action', () => {
    const { asFragment } = render(
      <Tag remove={<button type="button">Remove status</button>}>Active</Tag>,
    )

    expect(screen.getByRole('button', { name: 'Remove status' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })
})
