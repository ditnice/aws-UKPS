import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { Nav, NavProps } from './Nav'
import styles from './Nav.module.scss'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

const renderDefaultNav = (navProps?: Partial<NavProps>) => {
  const defaultNavProps: NavProps = {
    links: [
      { href: '/test1', label: 'Test link 1' },
      { href: '/test2', label: 'Test link 2' },
    ],
    isExpanded: false,
  }
  return render(<Nav {...defaultNavProps} {...navProps} />)
}

describe('Nav', () => {
  it('renders collapsed', () => {
    const { asFragment } = renderDefaultNav()

    const navigation = screen.getByRole('navigation', { name: 'primary navigation' })
    expect(navigation.parentElement).toHaveAttribute('id', 'header-menu')
    expect(navigation.parentElement).not.toHaveClass(styles.wrapperExpanded)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders expanded', () => {
    const { asFragment } = renderDefaultNav({ isExpanded: true })

    const navigation = screen.getByRole('navigation', { name: 'primary navigation' })
    expect(navigation.parentElement).toHaveClass(styles.wrapperExpanded)
    expect(asFragment()).toMatchSnapshot()
  })
})
