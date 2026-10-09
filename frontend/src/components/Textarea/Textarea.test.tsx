import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { inputWidthCases } from '@/test-utils/inputWidthCases'

import { Textarea } from './Textarea'

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
    render(
      <Textarea label="Description" name="description" style={{ color: 'red' }} width="full" />,
    )

    const textarea = screen.getByLabelText('Description')
    expect(textarea.style.width).toBe('100%')
    expect(textarea.style.color).toBe('red')
  })

  it('forwards other textarea props', () => {
    render(
      <Textarea
        hint="Include all relevant details"
        label="Description"
        name="description"
        placeholder="Enter a description"
        rows={8}
      />,
    )

    const textarea = screen.getByLabelText('Description') as HTMLTextAreaElement
    expect(screen.getByText('Include all relevant details')).toBeInTheDocument()
    expect(textarea.placeholder).toBe('Enter a description')
    expect(textarea.rows).toBe(8)
  })

  it.each(inputWidthCases)(
    'renders the $width width variant',
    ({ width, maxWidth, fluidWidth }) => {
      render(<Textarea label="Description" name="description" width={width} />)

      const textarea = screen.getByLabelText('Description')
      expect(textarea.style.maxWidth).toBe(maxWidth)
      expect(textarea.style.width).toBe(fluidWidth)
    },
  )
})
