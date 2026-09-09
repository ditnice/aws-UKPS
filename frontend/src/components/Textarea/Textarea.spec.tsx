import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Textarea } from './Textarea'

import type { TextareaWidth } from './Textarea'

const widths: TextareaWidth[] = [
  2,
  3,
  4,
  5,
  10,
  20,
  30,
  'full',
  'three-quarters',
  'two-thirds',
  'one-half',
  'one-third',
  'one-quarter',
]

afterEach(cleanup)

describe('Textarea', () => {
  it('renders an unmodified design system textarea by default', () => {
    const { asFragment } = render(<Textarea label="Description" name="description" />)

    const textarea = screen.getByLabelText('Description')
    expect(textarea.style.maxWidth).toBe('')
    expect(textarea.style.width).toBe('')
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a max-width for a fixed width', () => {
    const { asFragment } = render(<Textarea label="Description" name="description" width={10} />)

    const textarea = screen.getByLabelText('Description')
    expect(textarea.style.maxWidth).toBe('11.5em')
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a width for a fluid width', () => {
    const { asFragment } = render(
      <Textarea label="Description" name="description" width="one-half" />,
    )

    const textarea = screen.getByLabelText('Description')
    expect(textarea.style.width).toBe('50%')
    expect(asFragment()).toMatchSnapshot()
  })

  it('merges width styles with an explicit style prop', () => {
    const { asFragment } = render(
      <Textarea label="Description" name="description" style={{ color: 'red' }} width="full" />,
    )

    const textarea = screen.getByLabelText('Description')
    expect(textarea.style.width).toBe('100%')
    expect(textarea.style.color).toBe('red')
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards other textarea props', () => {
    const { asFragment } = render(
      <Textarea
        hint="Include all relevant details"
        label="Description"
        name="description"
        placeholder="Enter a description"
        rows={8}
      />,
    )

    const textarea = screen.getByLabelText('Description') as HTMLTextAreaElement
    expect(screen.getByText('Include all relevant details')).toBeDefined()
    expect(textarea.placeholder).toBe('Enter a description')
    expect(textarea.rows).toBe(8)
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(widths)('renders the %s width variant', (width) => {
    const { asFragment } = render(<Textarea label="Description" name="description" width={width} />)

    expect(screen.getByLabelText('Description')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
