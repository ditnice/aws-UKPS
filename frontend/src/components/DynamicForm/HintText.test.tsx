import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { HintText } from './HintText'

describe('HintText', () => {
  it.each([
    'Select all that apply.',
    'First paragraph.\n\nSecond paragraph.',
    'See [this](javascript:alert(1)).',
    'See [this](http://example.com).',
    'See [this](/portal).',
    '<a href="https://example.com">this</a>',
  ])('leaves text without an https link unchanged: %j', (text) => {
    const { container } = render(<HintText text={text} />)

    expect(container.textContent).toBe(text)
    expect(screen.queryByRole('link')).not.toBeInTheDocument()
  })

  it('opens a link in a new tab and keeps the text around it', () => {
    const { container } = render(<HintText text="Read [the guide](https://example.com/a?b=1#c)." />)

    const link = screen.getByRole('link', { name: 'the guide (opens in a new tab)' })
    expect(link).toHaveAttribute('href', 'https://example.com/a?b=1#c')
    expect(link).toHaveAttribute('target', '_blank')
    expect(link).toHaveAttribute('rel', 'noopener noreferrer')
    expect(container.textContent).toBe('Read the guide (opens in a new tab).')
  })

  it('supports several links across paragraphs', () => {
    const { container } = render(
      <HintText text={'[One](https://example.com/1)\n\n[Two](https://example.com/2)'} />,
    )

    expect(screen.getAllByRole('link').map((link) => link.getAttribute('href'))).toEqual([
      'https://example.com/1',
      'https://example.com/2',
    ])
    expect(container.textContent).toContain('\n\n')
  })
})
