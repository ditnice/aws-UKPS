import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest'

import { postAuthSignOut } from '@/client/generated'
import { router } from '@/test-utils/nextNavigation'

import { Account } from './Account'

const { push: mockPush } = router

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  postAuthSignOut: vi.fn(),
}))

function clearCookies() {
  document.cookie = 'csrf_token=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/'
}
afterEach(() => {
  clearCookies()
  vi.restoreAllMocks()
})

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('Account', () => {
  it('renders a sign in link when there is no session cookie', async () => {
    render(<Account />)

    const link = await screen.findByRole('link', { name: 'Sign in' })
    expect(link.getAttribute('href')).toBe('/auth/sign-in')
  })

  it('signs out through the backend API and redirects home', async () => {
    document.cookie = 'csrf_token=abc%20123; path=/'
    // The backend's Set-Cookie response expires the session cookies.
    vi.mocked(postAuthSignOut).mockImplementation(() => {
      clearCookies()
      return Promise.resolve({ data: undefined, error: undefined }) as ReturnType<
        typeof postAuthSignOut
      >
    })

    render(<Account />)
    await user.click(await screen.findByRole('button', { name: 'Sign out' }))

    await waitFor(() => expect(mockPush).toHaveBeenCalledWith('/'))
    expect(await screen.findByRole('link', { name: 'Sign in' })).toBeInTheDocument()
    expect(postAuthSignOut).toHaveBeenCalledWith({
      credentials: 'include',
      headers: { 'X-CSRF-Token': 'abc 123' },
    })
  })

  it('stays on the page when the backend rejects the sign-out', async () => {
    document.cookie = 'csrf_token=abc; path=/'
    vi.mocked(postAuthSignOut).mockResolvedValue({
      data: undefined,
      error: { title: 'CSRF validation failed' },
    } as Awaited<ReturnType<typeof postAuthSignOut>>)
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})

    render(<Account />)
    await user.click(await screen.findByRole('button', { name: 'Sign out' }))

    await waitFor(() => expect(consoleError).toHaveBeenCalledOnce())
    expect(mockPush).not.toHaveBeenCalled()
  })

  it('stays on the page when the sign-out request fails', async () => {
    document.cookie = 'csrf_token=abc; path=/'
    vi.mocked(postAuthSignOut).mockRejectedValue(new TypeError('Failed to fetch'))
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})

    render(<Account />)
    await user.click(await screen.findByRole('button', { name: 'Sign out' }))

    await waitFor(() => expect(consoleError).toHaveBeenCalledOnce())
    expect(mockPush).not.toHaveBeenCalled()
  })
})
