import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { Button, ButtonGroup } from './Button'
import styles from './Button.module.scss'

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

describe('ButtonGroup', () => {
  it('renders multiple buttons in a group', () => {
    const { asFragment } = render(
      <ButtonGroup className="custom-class">
        <Button>Continue</Button>
        <Button variant="secondary">Cancel</Button>
      </ButtonGroup>,
    )

    const group = screen.getByRole('button', { name: 'Continue' }).parentElement
    expect(group?.classList.contains(styles.buttonGroup)).toBe(true)
    expect(group?.classList.contains('custom-class')).toBe(true)
    expect(screen.getAllByRole('button')).toHaveLength(2)
    expect(asFragment()).toMatchSnapshot()
  })
})
