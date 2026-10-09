import { render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ApplicationLayout } from './ApplicationLayout'

type MockComponentProps = { children: import('react').ReactNode }

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/image', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextImageMock,
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@nice-digital/nds-container', () => ({
  Container: ({ children }: MockComponentProps) => <div data-testid="container">{children}</div>,
}))
describe('ApplicationLayout', () => {
  it('renders the application chrome around children', () => {
    render(<ApplicationLayout>Page content</ApplicationLayout>)

    const main = screen.getByRole('main')
    expect(within(main).getByText('Page content')).toBeInTheDocument()
    expect(screen.getByRole('banner', { name: 'Site header' })).toBeInTheDocument()
    expect(screen.getByRole('contentinfo')).toBeInTheDocument()
    expect(main).toHaveAttribute('id', 'content-start')
    expect(screen.getByRole('link', { name: 'Skip to content' })).toHaveAttribute(
      'href',
      '#content-start',
    )
  })
})
