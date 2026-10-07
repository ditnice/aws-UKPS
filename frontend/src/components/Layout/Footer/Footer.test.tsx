import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { Footer } from './Footer'

vi.mock('./Legal/Legal', () => ({
  Legal: () => <div data-testid="legal" />,
}))
describe('Footer', () => {
  it('renders a contentinfo landmark', () => {
    const { asFragment } = render(<Footer />)

    expect(screen.getByRole('contentinfo')).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('has the footer data-component attribute', () => {
    render(<Footer />)

    expect(screen.getByRole('contentinfo').getAttribute('data-component')).toBe('footer')
  })

  it('renders the placeholder content', () => {
    render(<Footer />)

    expect(screen.getByText('Footer placeholder content')).toBeInTheDocument()
  })

  it('renders Legal as a child', () => {
    render(<Footer />)

    expect(screen.getByTestId('legal')).toBeInTheDocument()
  })
})
