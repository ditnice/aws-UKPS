import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { Header } from './Header'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/image', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextImageMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('Header', () => {
  it('renders collapsed by default', () => {
    render(<Header skipLinkId="content-start" />)

    const toggle = screen.getByRole('button', { name: 'Expand site menu' })
    expect(toggle).toHaveAttribute('aria-expanded', 'false')
    expect(toggle).toHaveAttribute('aria-controls', 'header-menu')
    expect(document.getElementById('header-menu')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Skip to content' }).getAttribute('href')).toBe(
      '#content-start',
    )
  })

  it('renders expanded after the mobile menu button is clicked', async () => {
    render(<Header skipLinkId="content-start" />)

    await user.click(screen.getByRole('button', { name: 'Expand site menu' }))

    expect(screen.getByRole('button', { name: 'Close site menu' })).toHaveAttribute(
      'aria-expanded',
      'true',
    )
  })
})
