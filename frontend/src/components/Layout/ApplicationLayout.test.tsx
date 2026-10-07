import { render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

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
  beforeEach(() => {
    vi.useFakeTimers({ toFake: ['Date'] })
    vi.setSystemTime(new Date('2026-06-15T12:00:00Z'))
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('renders the application chrome around children', () => {
    const { asFragment } = render(<ApplicationLayout>Page content</ApplicationLayout>)

    expect(screen.getByText('Page content')).toBeInTheDocument()
    expect(screen.getByRole('main')).toBeInTheDocument()
    expect(screen.getByRole('contentinfo')).toBeInTheDocument()
    expect(document.getElementById('content-start')).not.toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })
})
