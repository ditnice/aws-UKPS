import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { PrintPageLink } from './PrintPageLink'

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})

describe('PrintPageLink', () => {
  it('prints the page from a semantic button', () => {
    const print = vi.spyOn(window, 'print').mockImplementation(() => undefined)
    const { asFragment } = render(<PrintPageLink />)

    const button = screen.getByRole('button', { name: 'Print page' })
    expect(button.getAttribute('type')).toBe('button')
    expect(screen.queryByRole('link', { name: 'Print page' })).toBeNull()

    fireEvent.click(button)
    expect(print).toHaveBeenCalledTimes(1)
    expect(asFragment()).toMatchSnapshot()
  })
})
