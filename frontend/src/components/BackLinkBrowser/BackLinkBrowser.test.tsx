import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { BackLinkBrowser } from './BackLinkBrowser'

const mockBack = vi.fn()

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    back: mockBack,
  }),
}))

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('BackLinkBrowser', () => {
  it('renders the default link text', () => {
    const { asFragment } = render(<BackLinkBrowser />)

    expect(screen.getByRole('link', { name: 'Back' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('navigates back without following the link', () => {
    const { asFragment } = render(<BackLinkBrowser />)

    const shouldNavigate = fireEvent.click(screen.getByRole('link', { name: 'Back' }))

    expect(shouldNavigate).toBe(false)
    expect(mockBack).toHaveBeenCalledOnce()
    expect(asFragment()).toMatchSnapshot()
  })
})
