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

  it('renders the default portal links', () => {
    navigationState.pathname = '/portal'

    render(<NavLinks organisationId={12} />)

    expect(screen.getAllByRole('link').map((link) => link.textContent)).toEqual([
      'Dashboard',
      'Components',
      'View and manage records',
      'Manage my organisation',
      'Manage my details',
    ])
    expect(screen.getByRole('link', { name: 'Manage my details' })).toHaveAttribute(
      'href',
      '/portal/user/me',
    )
  })

  it.each([
    ['/portal', 'Dashboard'],
    ['/portal/components/examples', 'Components'],
    ['/portal/organisations/12', 'Manage my organisation'],
    ['/portal/organisations/12/users/3/deactivate', 'Manage my organisation'],
    ['/portal/organisations/12/records', 'View and manage records'],
    ['/portal/organisations/12/records/7', 'View and manage records'],
    ['/portal/user/me/edit-details', 'Manage my details'],
  ])('marks only the most specific link as active for %s', (pathname, activeLabel) => {
    navigationState.pathname = pathname

    render(<NavLinks organisationId={12} />)

    expect(getActiveLinkLabels()).toEqual([activeLabel])
  })

  it.each([
    ['a page without a link', '/portal/organisations/switch', 12],
    ['another organisation', '/portal/organisations/13/records', 12],
  ])('marks no link as active on %s', (_, pathname, organisationId) => {
    navigationState.pathname = pathname

    render(<NavLinks organisationId={organisationId} />)

    expect(getActiveLinkLabels()).toEqual([])
  })

  it("links organisation pages to the user's current organisation", () => {
    navigationState.pathname = '/portal/user/me'

    render(<NavLinks organisationId={12} />)

    expect(screen.getByRole('link', { name: 'View and manage records' })).toHaveAttribute(
      'href',
      '/portal/organisations/12/records',
    )
    expect(screen.getByRole('link', { name: 'Manage my organisation' })).toHaveAttribute(
      'href',
      '/portal/organisations/12',
    )
  })

  it('omits links that need a selected organisation when the user has none', () => {
    navigationState.pathname = '/portal/organisations/select'

    render(<NavLinks />)

    expect(screen.getAllByRole('link').map((link) => link.textContent)).toEqual([
      'Dashboard',
      'Components',
    ])
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

function getActiveLinkLabels() {
  return screen
    .getAllByRole('link')
    .filter((link) => link.getAttribute('aria-current') === 'page')
    .map((link) => link.textContent)
}
