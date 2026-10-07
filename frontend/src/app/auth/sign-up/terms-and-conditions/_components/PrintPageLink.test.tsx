import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest'

import { PrintPageLink } from './PrintPageLink'

afterEach(() => {
  vi.restoreAllMocks()
})

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('PrintPageLink', () => {
  it('prints the page from a semantic button', async () => {
    const print = vi.spyOn(window, 'print').mockImplementation(() => undefined)
    const { asFragment } = render(<PrintPageLink />)

    const button = screen.getByRole('button', { name: 'Print page' })
    expect(button.getAttribute('type')).toBe('button')
    expect(screen.queryByRole('link', { name: 'Print page' })).toBeNull()
    expect(asFragment()).toMatchSnapshot()

    await user.click(button)
    expect(print).toHaveBeenCalledTimes(1)
  })
})
