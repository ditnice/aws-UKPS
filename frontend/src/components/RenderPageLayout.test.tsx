import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import RenderPageLayout from './RenderPageLayout'

describe('RenderPageLayout', () => {
  it('renders the page title as the only level 1 heading', () => {
    render(<RenderPageLayout blocks={[]} title="UK PharmaScan" />)

    expect(screen.getAllByRole('heading', { level: 1 })).toHaveLength(1)
    expect(screen.getByRole('heading', { level: 1, name: 'UK PharmaScan' })).toBeInTheDocument()
  })

  it('renders the page header fields when set', () => {
    const { container } = render(
      <RenderPageLayout
        blocks={[]}
        pageHeader={{
          preheading: 'About',
          lead: 'Who we are',
          description: 'First paragraph\n\nSecond paragraph',
        }}
        title="About us"
      />,
    )

    expect(container.querySelector('.page-header__pre-heading')?.textContent).toBe('About')
    expect(container.querySelector('.page-header__lead')?.textContent).toBe('Who we are')
    expect(container.querySelectorAll('.page-header__description p')).toHaveLength(2)
  })

  it('omits the description when it is not set', () => {
    const { container } = render(<RenderPageLayout blocks={[]} pageHeader={{}} title="About us" />)

    expect(container.querySelector('.page-header__description')).toBeNull()
  })
})
