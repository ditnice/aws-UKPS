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
    const { asFragment } = render(<Header skipLinkId="content-start" />)

    expect(screen.getByRole('button', { name: 'Expand site menu' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Skip to content' }).getAttribute('href')).toBe(
      '#content-start',
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders expanded after the mobile menu button is clicked', async () => {
    const { asFragment } = render(<Header skipLinkId="content-start" />)

    await user.click(screen.getByRole('button', { name: 'Expand site menu' }))

    expect(screen.getByRole('button', { name: 'Close site menu' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })
})
