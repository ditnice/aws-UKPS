import { cleanup, render } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { Nav } from './Nav'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('next/navigation', () => ({
  usePathname: () => '/',
}))

afterEach(cleanup)

describe('Nav', () => {
  it('renders collapsed', () => {
    const { asFragment } = render(<Nav isExpanded={false} />)

    expect(asFragment()).toMatchSnapshot()
  })

  it('renders expanded', () => {
    const { asFragment } = render(<Nav isExpanded />)

    expect(asFragment()).toMatchSnapshot()
  })
})
