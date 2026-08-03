import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Input } from './Input'

afterEach(cleanup)

describe('Input', () => {
  it('renders an unmodified design system input by default', () => {
    render(<Input label="First name" name="firstname" />)

    const input = screen.getByLabelText('First name')
    expect(input.style.maxWidth).toBe('')
    expect(input.style.width).toBe('')
  })

  it('applies a max-width for a fixed width', () => {
    render(<Input label="Age" name="age" width={10} />)

    const input = screen.getByLabelText('Age')
    expect(input.style.maxWidth).toBe('11.5em')
  })

  it('applies a width for a fluid width', () => {
    render(<Input label="Age" name="age" width="one-half" />)

    const input = screen.getByLabelText('Age')
    expect(input.style.width).toBe('50%')
  })

  it('merges width styles with an explicit style prop', () => {
    render(<Input label="Age" name="age" width="full" style={{ color: 'red' }} />)

    const input = screen.getByLabelText('Age')
    expect(input.style.width).toBe('100%')
    expect(input.style.color).toBe('red')
  })

  it('forwards other input props', () => {
    render(<Input label="Age" name="age" hint="Please enter in years" />)

    expect(screen.getByText('Please enter in years')).toBeDefined()
  })
})
