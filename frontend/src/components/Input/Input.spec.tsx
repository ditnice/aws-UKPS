import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Input } from './Input'

import type { InputWidth } from './Input'

const widths: InputWidth[] = [
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

describe('Input', () => {
  it('renders an unmodified design system input by default', () => {
    const { asFragment } = render(<Input label="First name" name="firstname" />)

    const input = screen.getByLabelText('First name')
    expect(input.style.maxWidth).toBe('')
    expect(input.style.width).toBe('')
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a max-width for a fixed width', () => {
    const { asFragment } = render(<Input label="Age" name="age" width={10} />)

    const input = screen.getByLabelText('Age')
    expect(input.style.maxWidth).toBe('11.5em')
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a width for a fluid width', () => {
    const { asFragment } = render(<Input label="Age" name="age" width="one-half" />)

    const input = screen.getByLabelText('Age')
    expect(input.style.width).toBe('50%')
    expect(asFragment()).toMatchSnapshot()
  })

  it('merges width styles with an explicit style prop', () => {
    const { asFragment } = render(
      <Input label="Age" name="age" width="full" style={{ color: 'red' }} />,
    )

    const input = screen.getByLabelText('Age')
    expect(input.style.width).toBe('100%')
    expect(input.style.color).toBe('red')
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards other input props', () => {
    const { asFragment } = render(<Input label="Age" name="age" hint="Please enter in years" />)

    expect(screen.getByText('Please enter in years')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(widths)('renders the %s width variant', (width) => {
    const { asFragment } = render(<Input label="Age" name="age" width={width} />)

    expect(screen.getByLabelText('Age')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
