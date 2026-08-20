import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { Header } from './Header'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/image', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextImageMock,
}))

vi.mock('next/navigation', () => ({
  usePathname: () => '/',
}))

afterEach(cleanup)

describe('Header', () => {
  it('renders collapsed by default', () => {
    const { asFragment } = render(<Header skipLinkId="content-start" />)

    expect(screen.getByRole('button', { name: 'Expand site menu' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Skip to content' }).getAttribute('href')).toBe(
      '#content-start',
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders expanded after the mobile menu button is clicked', () => {
    const { asFragment } = render(<Header skipLinkId="content-start" />)

    fireEvent.click(screen.getByRole('button', { name: 'Expand site menu' }))

    expect(screen.getByRole('button', { name: 'Close site menu' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
