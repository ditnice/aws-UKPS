import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { navigationState } from '@/test-utils/nextNavigation'

import { NavLinks } from './NavLinks'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

describe('NavLinks', () => {
  it('renders root links for the root path', () => {
    navigationState.pathname = '/'

    render(<NavLinks />)

    expect(screen.getByRole('link', { name: 'Home' }).getAttribute('aria-current')).toBe('page')
    expect(screen.getByRole('link', { name: 'Home' })).toHaveAttribute('href', '/')
    expect(screen.getByRole('link', { name: 'About' })).toHaveAttribute('href', '/about-us')
    expect(screen.getByRole('link', { name: 'About' })).not.toHaveAttribute('aria-current')
    expect(screen.queryByRole('link', { name: 'Dashboard' })).not.toBeInTheDocument()
  })

  it('renders portal links for the portal path', () => {
    navigationState.pathname = '/portal'

    render(<NavLinks />)

    expect(screen.getByRole('link', { name: 'Dashboard' }).getAttribute('aria-current')).toBe(
      'page',
    )
    expect(screen.getByRole('link', { name: 'Components' }).hasAttribute('aria-current')).toBe(
      false,
    )
    expect(screen.getByRole('link', { name: 'Dashboard' })).toHaveAttribute('href', '/portal')
    expect(screen.getByRole('link', { name: 'Components' })).toHaveAttribute(
      'href',
      '/portal/components',
    )
    expect(screen.queryByRole('link', { name: 'Home' })).not.toBeInTheDocument()
  })

  it('marks nested portal links as active', () => {
    navigationState.pathname = '/portal/components/examples'

    render(<NavLinks />)

    expect(screen.getByRole('link', { name: 'Components' }).getAttribute('aria-current')).toBe(
      'page',
    )
  })

  it('renders custom root links', () => {
    render(<NavLinks rootLinks={[{ href: '/guidance', label: 'Guidance' }]} />)

    expect(screen.getByRole('link', { name: 'Guidance' })).toHaveAttribute('href', '/guidance')
  })

  it('renders custom portal links', () => {
    navigationState.pathname = '/portal/settings'

    render(<NavLinks portalLinks={[{ href: '/portal/settings', label: 'Settings' }]} />)

    expect(screen.getByRole('link', { name: 'Settings' }).getAttribute('aria-current')).toBe('page')
    expect(screen.getByRole('link', { name: 'Settings' })).toHaveAttribute(
      'href',
      '/portal/settings',
    )
  })
})
