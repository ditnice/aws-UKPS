import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Tag } from './Tag'

import type { TagColour } from './Tag'

afterEach(cleanup)

const colours: TagColour[] = [
  'grey',
  'green',
  'teal',
  'blue',
  'purple',
  'magenta',
  'red',
  'orange',
  'yellow',
]

describe('Tag', () => {
  it.each(colours)('renders the %s colour variant', (colour) => {
    const { asFragment } = render(<Tag colour={colour}>Status</Tag>)

    expect(screen.getByText('Status')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('wraps the design-system tag when a className is supplied', () => {
    const { asFragment, container } = render(
      <Tag className="additional-class" colour="green">
        Active
      </Tag>,
    )

    expect(container.firstElementChild?.classList.contains('additional-class')).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('merges consumer styles with the colour styles', () => {
    const { asFragment } = render(
      <Tag colour="blue" style={{ border: '1px solid red' }}>
        Draft
      </Tag>,
    )

    const tag = screen.getByText('Draft')
    expect(tag.style.border).toBe('1px solid red')
    expect(tag.style.backgroundColor).toBe('rgb(210, 226, 241)')
    expect(asFragment()).toMatchSnapshot()
  })

  it('passes through design-system modifier props and HTML attributes', () => {
    const { asFragment } = render(
      <Tag flush impact outline data-testid="priority-tag">
        High priority
      </Tag>,
    )

    expect(screen.getByTestId('priority-tag')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a remove action', () => {
    const { asFragment } = render(
      <Tag remove={<button type="button">Remove status</button>}>Active</Tag>,
    )

    expect(screen.getByRole('button', { name: 'Remove status' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
