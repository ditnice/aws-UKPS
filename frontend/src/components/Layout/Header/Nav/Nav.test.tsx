import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { navigationState } from '@/test-utils/nextNavigation'

import { Nav } from './Nav'
import styles from './Nav.module.scss'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
describe('Nav', () => {
  it('renders collapsed', () => {
    const { asFragment } = render(<Nav isExpanded={false} />)

    const navigation = screen.getByRole('navigation', { name: 'primary navigation' })
    expect(navigation.parentElement).toHaveAttribute('id', 'header-menu')
    expect(navigation.parentElement).not.toHaveClass(styles.wrapperExpanded)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders expanded', () => {
    const { asFragment } = render(<Nav isExpanded />)

    const navigation = screen.getByRole('navigation', { name: 'primary navigation' })
    expect(navigation.parentElement).toHaveClass(styles.wrapperExpanded)
    expect(asFragment()).toMatchSnapshot()
  })

  it('passes the current organisation to the portal links', () => {
    navigationState.pathname = '/portal'

    render(<Nav isExpanded={false} organisationId={12} />)

    expect(screen.getByRole('link', { name: 'Manage my organisation' })).toHaveAttribute(
      'href',
      '/portal/organisations/12',
    )
  })
})
