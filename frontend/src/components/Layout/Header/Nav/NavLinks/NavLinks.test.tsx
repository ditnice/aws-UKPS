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

    const { asFragment } = render(<NavLinks />)

    expect(screen.getByRole('link', { name: 'Home' }).getAttribute('aria-current')).toBe('page')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders portal links for the portal path', () => {
    navigationState.pathname = '/portal'

    const { asFragment } = render(<NavLinks />)

    expect(screen.getByRole('link', { name: 'Dashboard' }).getAttribute('aria-current')).toBe(
      'page',
    )
    expect(screen.getByRole('link', { name: 'Components' }).hasAttribute('aria-current')).toBe(
      false,
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('marks nested portal links as active', () => {
    navigationState.pathname = '/portal/components/examples'

    const { asFragment } = render(<NavLinks />)

    expect(screen.getByRole('link', { name: 'Components' }).getAttribute('aria-current')).toBe(
      'page',
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders custom root links', () => {
    const { asFragment } = render(
      <NavLinks rootLinks={[{ href: '/guidance', label: 'Guidance' }]} />,
    )

    expect(screen.getByRole('link', { name: 'Guidance' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders custom portal links', () => {
    navigationState.pathname = '/portal/settings'

    const { asFragment } = render(
      <NavLinks portalLinks={[{ href: '/portal/settings', label: 'Settings' }]} />,
    )

    expect(screen.getByRole('link', { name: 'Settings' }).getAttribute('aria-current')).toBe('page')
    expect(asFragment()).toMatchSnapshot()
  })
})
