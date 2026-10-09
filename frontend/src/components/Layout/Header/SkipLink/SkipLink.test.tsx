import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest'

import { SkipLink } from './SkipLink'

afterEach(() => {
  document.body.replaceChildren()
})

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('SkipLink', () => {
  it('renders a hash link and focuses the target when clicked', async () => {
    const target = document.createElement('main')
    target.id = 'content-start'
    target.scrollIntoView = vi.fn()
    document.body.append(target)

    const { asFragment } = render(<SkipLink to="#content-start">Skip to content</SkipLink>)

    expect(asFragment()).toMatchSnapshot()

    await user.click(screen.getByRole('link', { name: 'Skip to content' }))

    expect(target).toHaveFocus()
    expect(target.tabIndex).toBe(-1)
    expect(target.scrollIntoView).toHaveBeenCalledWith({ block: 'start' })
  })

  it('renders a hash link without changing focus when the target is missing', async () => {
    render(<SkipLink to="#missing">Missing target</SkipLink>)

    const link = screen.getByRole('link', { name: 'Missing target' })
    await user.tab()
    expect(link).toHaveFocus()
    await user.click(link)

    expect(link).toHaveFocus()
  })

  it('renders an empty hash link without changing focus when clicked', async () => {
    render(<SkipLink to="#">Empty target</SkipLink>)

    const link = screen.getByRole('link', { name: 'Empty target' })
    await user.tab()
    expect(link).toHaveFocus()
    await user.click(link)

    expect(link).toHaveFocus()
  })

  it('renders a normal link for non-hash destinations', () => {
    render(<SkipLink to="/accessibility">Accessibility help</SkipLink>)

    expect(screen.getByRole('link', { name: 'Accessibility help' }).getAttribute('href')).toBe(
      '/accessibility',
    )
  })
})
