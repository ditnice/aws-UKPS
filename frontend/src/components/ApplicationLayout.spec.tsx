import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ApplicationLayout } from './ApplicationLayout'

type MockComponentProps = { children: import('react').ReactNode }

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/image', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextImageMock,
}))

vi.mock('next/navigation', () => ({
  usePathname: () => '/',
}))

vi.mock('@nice-digital/global-nav', () => ({
  Footer: () => <footer data-testid="footer" />,
  Main: ({ children, id }: MockComponentProps & { id: string }) => <main id={id}>{children}</main>,
}))

vi.mock('@nice-digital/nds-container', () => ({
  Container: ({ children }: MockComponentProps) => <div data-testid="container">{children}</div>,
}))

afterEach(cleanup)

describe('ApplicationLayout', () => {
  it('renders the application chrome around children', () => {
    const { asFragment } = render(<ApplicationLayout>Page content</ApplicationLayout>)

    expect(screen.getByText('Page content')).toBeDefined()
    expect(screen.getByTestId('container')).toBeDefined()
    expect(screen.getByTestId('footer')).toBeDefined()
    expect(document.getElementById('content-start')).not.toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })
})
