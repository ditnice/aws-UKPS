import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import { BackLinkBrowser } from './BackLinkBrowser'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('BackLinkBrowser', () => {
  it('renders the default link text', () => {
    const { asFragment } = render(<BackLinkBrowser />)

    expect(screen.getByRole('link', { name: 'Back' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('navigates back without following the link', async () => {
    const { asFragment } = render(<BackLinkBrowser />)

    const link = screen.getByRole('link', { name: 'Back' })
    const onClick = vi.fn((event: Event) => {
      expect(event.defaultPrevented).toBe(true)
    })
    document.addEventListener('click', onClick, { once: true })
    await user.click(link)

    expect(onClick).toHaveBeenCalledOnce()
    expect(router.back).toHaveBeenCalledOnce()
    expect(asFragment()).toMatchSnapshot()
  })
})
