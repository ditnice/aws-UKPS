import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { Footer } from './Footer'

vi.mock('./Legal/Legal', () => ({
  Legal: () => <div data-testid="legal" />,
}))

afterEach(cleanup)

describe('Footer', () => {
  it('renders a contentinfo landmark', () => {
    const { asFragment } = render(<Footer />)

    expect(screen.getByRole('contentinfo')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('has the footer data-component attribute', () => {
    const { asFragment } = render(<Footer />)

    expect(screen.getByRole('contentinfo').getAttribute('data-component')).toBe('footer')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders the placeholder content', () => {
    const { asFragment } = render(<Footer />)

    expect(screen.getByText('Footer placeholder content')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders Legal as a child', () => {
    const { asFragment } = render(<Footer />)

    expect(screen.getByTestId('legal')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
