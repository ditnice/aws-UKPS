import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { navigationState } from '@/test-utils/nextNavigation'

import { ShowOnce } from './ShowOnce'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

const banner = <p>Record submitted</p>

beforeEach(() => {
  window.history.replaceState(null, '', '/')
})

describe('ShowOnce', () => {
  it('shows the banner when a banner parameter is in the URL on arrival', () => {
    navigationState.searchParams = new URLSearchParams({ action: 'submitted' })

    render(<ShowOnce>{banner}</ShowOnce>)

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('shows nothing when the page is restored with a clean URL (Back or Forward)', () => {
    render(<ShowOnce>{banner}</ShowOnce>)

    expect(screen.queryByText('Record submitted')).not.toBeInTheDocument()
  })

  it('keeps showing the banner after the parameter has been removed from the URL', () => {
    navigationState.searchParams = new URLSearchParams({ action: 'submitted' })
    const { rerender } = render(<ShowOnce>{banner}</ShowOnce>)

    navigationState.searchParams = new URLSearchParams()
    rerender(<ShowOnce>{banner}</ShowOnce>)

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('shows the banner when a parameter appears after the page was mounted without one', () => {
    const { rerender } = render(<ShowOnce>{banner}</ShowOnce>)
    expect(screen.queryByText('Record submitted')).not.toBeInTheDocument()

    navigationState.searchParams = new URLSearchParams({ action: 'submitted' })
    rerender(<ShowOnce>{banner}</ShowOnce>)

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('removes the banner parameters from the address bar and keeps the others', () => {
    window.history.replaceState(null, '', '/records?action=submitted&userId=4&recordType=medicine')
    navigationState.searchParams = new URLSearchParams({ action: 'submitted', userId: '4' })

    render(<ShowOnce>{banner}</ShowOnce>)

    expect(window.location.pathname).toBe('/records')
    expect(window.location.search).toBe('?recordType=medicine')
  })

  it('leaves the address bar alone when there is no banner parameter', () => {
    window.history.replaceState(null, '', '/records?recordType=medicine')
    const replaceState = vi.spyOn(window.history, 'replaceState')

    render(<ShowOnce>{banner}</ShowOnce>)

    expect(replaceState).not.toHaveBeenCalled()
  })
})
