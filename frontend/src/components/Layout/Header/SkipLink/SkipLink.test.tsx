import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SkipLink } from './SkipLink'

afterEach(() => {
  document.body.replaceChildren()
  cleanup()
})

describe('SkipLink', () => {
  it('renders a hash link and focuses the target when clicked', () => {
    const target = document.createElement('main')
    target.id = 'content-start'
    target.scrollIntoView = vi.fn()
    document.body.append(target)

    const { asFragment } = render(<SkipLink to="#content-start">Skip to content</SkipLink>)

    fireEvent.click(screen.getByRole('link', { name: 'Skip to content' }))

    expect(document.activeElement).toBe(target)
    expect(target.tabIndex).toBe(-1)
    expect(target.scrollIntoView).toHaveBeenCalledWith({ block: 'start' })
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a hash link without changing focus when the target is missing', () => {
    const { asFragment } = render(<SkipLink to="#missing">Missing target</SkipLink>)

    fireEvent.click(screen.getByRole('link', { name: 'Missing target' }))

    expect(document.activeElement).toBe(document.body)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders an empty hash link without changing focus when clicked', () => {
    const { asFragment } = render(<SkipLink to="#">Empty target</SkipLink>)

    fireEvent.click(screen.getByRole('link', { name: 'Empty target' }))

    expect(document.activeElement).toBe(document.body)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a normal link for non-hash destinations', () => {
    const { asFragment } = render(<SkipLink to="/accessibility">Accessibility help</SkipLink>)

    expect(screen.getByRole('link', { name: 'Accessibility help' }).getAttribute('href')).toBe(
      '/accessibility',
    )
    expect(asFragment()).toMatchSnapshot()
  })
})
