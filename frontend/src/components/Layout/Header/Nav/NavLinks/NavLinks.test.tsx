import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { navigationState } from '@/test-utils/nextNavigation'

import { NavLinks, type NavLinkItem } from './NavLinks'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

type StringLabelledNavLinkItem = NavLinkItem & { stringLabel: string }

const links: StringLabelledNavLinkItem[] = [
  { href: '/', label: <>Home</>, stringLabel: 'Home' },
  { href: '/about-us', label: 'About', stringLabel: 'About' },
  { href: '/portal', label: 'Dashboard', stringLabel: 'Dashboard' },
  { href: '/portal/components', label: 'Components', stringLabel: 'Components' },
]

const getLinkByLabel = (label: string) => screen.getByRole('link', { name: label })
const assertLinkIsActive = (label: string) =>
  expect(getLinkByLabel(label)).toHaveAttribute('aria-current', 'page')
const assertLinkIsNotActive = (label: string) =>
  expect(getLinkByLabel(label)).not.toHaveAttribute('aria-current')
const assertOnlySpecifiedLinkIsActive = (expectedActiveLink: string) => {
  assertLinkIsActive(expectedActiveLink)
  links
    .filter((x) => x.stringLabel != expectedActiveLink)
    .forEach((x) => assertLinkIsNotActive(x.stringLabel))
}
const assertAllLinksAreNotActive = () => {
  links.map((x) => x.stringLabel).forEach(assertLinkIsNotActive)
}

describe('NavLinks', () => {
  it('renders a list item and link for each provided link', () => {
    navigationState.pathname = '/'

    render(<NavLinks links={links} />)

    expect(screen.getAllByRole('listitem')).toHaveLength(links.length)

    links.forEach(({ href, stringLabel }) => {
      const link = getLinkByLabel(stringLabel)
      expect(link).toBeInTheDocument()
      expect(link).toHaveAttribute('href', href)
    })
  })

  it('renders nothing but an empty list when no links are provided', () => {
    navigationState.pathname = '/'

    render(<NavLinks links={[]} />)

    expect(screen.getByRole('list')).toBeEmptyDOMElement()
    expect(screen.queryAllByRole('link')).toHaveLength(0)
  })

  it('links the list to the header menu button', () => {
    navigationState.pathname = '/'

    render(<NavLinks links={links} />)

    expect(screen.getByRole('list')).toHaveAttribute('aria-labelledby', 'header-menu-button')
  })

  it('marks the root link as active only on the root path', () => {
    navigationState.pathname = '/'

    render(<NavLinks links={links} />)

    assertOnlySpecifiedLinkIsActive('Home')
  })

  it('does not mark the root link as active on other paths', () => {
    navigationState.pathname = '/about-us'

    render(<NavLinks links={links} />)

    expect(getLinkByLabel('Home')).not.toHaveAttribute('aria-current')
    expect(getLinkByLabel('About')).toHaveAttribute('aria-current', 'page')
  })

  it('marks only the exact matching link as active', () => {
    navigationState.pathname = '/portal'

    render(<NavLinks links={links} />)

    assertOnlySpecifiedLinkIsActive('Dashboard')
  })

  it('does not mark a link as active for nested paths', () => {
    navigationState.pathname = '/portal/components/examples'

    render(<NavLinks links={links} />)

    assertAllLinksAreNotActive()
  })

  it('has no active link when the path matches none of the links', () => {
    navigationState.pathname = '/somewhere-else'

    render(<NavLinks links={links} />)

    assertAllLinksAreNotActive()
  })

  it('ignores query strings in the link href when determining the active link', () => {
    navigationState.pathname = '/portal/user/me'

    const link = { href: '/portal/user/me?tab=details', label: 'Manage my details' }
    render(
      <NavLinks links={[{ href: '/portal/user/me?tab=details', label: 'Manage my details' }]} />,
    )

    const linkElement = getLinkByLabel(link.label)
    expect(linkElement).toHaveAttribute('href', link.href)
    assertLinkIsActive(link.label)
  })

  it('ignores hash fragments in the link href when determining the active link', () => {
    navigationState.pathname = '/guidance'

    const link = { href: '/guidance#faqs', label: 'Guidance' }
    render(<NavLinks links={[link]} />)

    const linkElement = getLinkByLabel(link.label)
    expect(linkElement).toHaveAttribute('href', link.href)
    assertLinkIsActive(link.label)
  })

  it('renders custom links and marks the current one as active', () => {
    navigationState.pathname = '/portal/settings'

    render(
      <NavLinks
        links={[
          { href: '/portal/settings', label: 'Settings' },
          { href: '/portal/profile', label: 'Profile' },
        ]}
      />,
    )

    expect(getLinkByLabel('Settings')).toHaveAttribute('aria-current', 'page')
    expect(getLinkByLabel('Settings')).toHaveAttribute('href', '/portal/settings')
    expect(getLinkByLabel('Profile')).not.toHaveAttribute('aria-current')
  })
})
