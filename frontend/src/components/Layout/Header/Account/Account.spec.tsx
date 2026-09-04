import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Account } from './Account'

function clearCookies() {
  document.cookie = 'csrf_token=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/'
}

afterEach(() => {
  clearCookies()
  cleanup()
})

describe('Account', () => {
  it('renders a sign in link when there is no session cookie', async () => {
    render(<Account />)

    const link = await screen.findByRole('link', { name: 'Sign in' })
    expect(link.getAttribute('href')).toBe('/auth/sign-in')
  })

  it('renders a sign out form when a session cookie is present', async () => {
    document.cookie = 'csrf_token=abc; path=/'

    render(<Account />)

    const button = await screen.findByRole('button', { name: 'Sign out' })
    const form = button.closest('form')
    expect(form?.getAttribute('action')).toBe('/auth/sign-out')
    expect(form?.getAttribute('method')).toBe('POST')
  })
})
