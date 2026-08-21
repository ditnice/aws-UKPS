import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { Button } from './Button'

afterEach(cleanup)

describe('Button', () => {
  it('renders a design-system button by default', () => {
    const { asFragment } = render(<Button>Continue</Button>)

    const button = screen.getByRole('button', { name: 'Continue' })
    expect(button.className).toMatch(/\bbtn\b/)
    expect(button.getAttribute('type')).toBe('button')
    expect(asFragment()).toMatchSnapshot()
  })

  it('passes design-system variants through', () => {
    const { asFragment } = render(<Button variant="cta">Continue</Button>)

    const button = screen.getByRole('button', { name: 'Continue' })
    expect(button.className).toMatch(/\bbtn\b/)
    expect(button.className).toMatch(/\bbtn--cta\b/)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders the link variant as a semantic button', () => {
    const handleClick = vi.fn()
    const { asFragment } = render(
      <Button className="custom-class" variant="link" onClick={handleClick}>
        Print page
      </Button>,
    )

    const button = screen.getByRole('button', { name: 'Print page' })
    expect(button.getAttribute('type')).toBe('button')
    expect(button.className).toMatch(/\bbtn\b/)
    expect(button.classList.contains('custom-class')).toBe(true)
    expect(screen.queryByRole('link', { name: 'Print page' })).toBeNull()

    fireEvent.click(button)
    expect(handleClick).toHaveBeenCalledTimes(1)
    expect(asFragment()).toMatchSnapshot()
  })
})
