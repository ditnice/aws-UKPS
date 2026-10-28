import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { Header, type HeaderProps } from './Header'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/image', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextImageMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

// Account is a child component with its own dependencies (session, etc.),
// so stub it out to keep these tests focused on Header.
vi.mock('./Account/Account', () => ({
  Account: () => <div data-testid="account" />,
}))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

const defaultLinks: HeaderProps['links'] = [
  { href: '/test1', label: 'Test link 1' },
  { href: '/test2', label: 'Test link 2' },
]

const renderHeader = (props: Partial<HeaderProps> = {}) =>
  render(<Header skipLinkId="content-start" links={defaultLinks} {...props} />)

describe('Header', () => {
  it('renders collapsed by default', () => {
    renderHeader()

    const toggle = screen.getByRole('button', { name: 'Expand site menu' })
    expect(toggle).toHaveAttribute('aria-expanded', 'false')
    expect(toggle).toHaveAttribute('aria-controls', 'header-menu')
    expect(toggle).toHaveAttribute('aria-haspopup', 'menu')
    expect(toggle).toHaveTextContent('Menu')
    expect(document.getElementById('header-menu')).toBeInTheDocument()
  })

  it('renders expanded after the mobile menu button is clicked', async () => {
    renderHeader()

    await user.click(screen.getByRole('button', { name: 'Expand site menu' }))

    const toggle = screen.getByRole('button', { name: 'Close site menu' })
    expect(toggle).toHaveAttribute('aria-expanded', 'true')
    expect(toggle).toHaveTextContent('Close')
  })

  it('collapses again when the mobile menu button is clicked a second time', async () => {
    renderHeader()

    await user.click(screen.getByRole('button', { name: 'Expand site menu' }))
    await user.click(screen.getByRole('button', { name: 'Close site menu' }))

    expect(screen.getByRole('button', { name: 'Expand site menu' })).toHaveAttribute(
      'aria-expanded',
      'false',
    )
  })

  it('renders the accessibility links', () => {
    renderHeader()

    const a11yList = screen.getByRole('list', { name: 'Accessibility links' })
    expect(within(a11yList).getByRole('link', { name: 'Skip to content' })).toHaveAttribute(
      'href',
      '#content-start',
    )
    expect(within(a11yList).getByRole('link', { name: 'Accessibility help' })).toHaveAttribute(
      'href',
      '/accessibility',
    )
  })

  it('renders the homepage link with an accessible name', () => {
    renderHeader()

    expect(
      screen.getByRole('link', {
        name: 'NICE: National Institute for Health and Care Excellence homepage',
      }),
    ).toHaveAttribute('href', '/')
  })

  it('renders the account component', () => {
    renderHeader()

    expect(screen.getByTestId('account')).toBeInTheDocument()
  })

  it('renders the navigation links', () => {
    renderHeader()

    expect(screen.getByRole('link', { name: 'Test link 1' })).toHaveAttribute('href', '/test1')
    expect(screen.getByRole('link', { name: 'Test link 2' })).toHaveAttribute('href', '/test2')
  })

  it('renders the header extension when provided', () => {
    renderHeader({ headerExtension: <div data-testid="header-extension">Extension</div> })

    expect(screen.getByTestId('header-extension')).toHaveTextContent('Extension')
  })

  it('renders the header extension outside the header landmark', () => {
    renderHeader({ headerExtension: <div data-testid="header-extension">Extension</div> })

    const header = screen.getByRole('banner')
    expect(within(header).queryByTestId('header-extension')).not.toBeInTheDocument()
  })

  it('does not render a header extension when none is provided', () => {
    renderHeader()

    expect(screen.queryByTestId('header-extension')).not.toBeInTheDocument()
  })
})
