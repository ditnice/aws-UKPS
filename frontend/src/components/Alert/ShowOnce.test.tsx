import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { navigationState } from '@/test-utils/nextNavigation'

import { ShowOnce } from './ShowOnce'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

const banner = <p>Record submitted</p>
const params = ['action'] as const

beforeEach(() => {
  window.history.replaceState(null, '', '/')
})

describe('ShowOnce', () => {
  it('shows the banner when its parameter is in the URL on arrival', () => {
    navigationState.searchParams = new URLSearchParams({ action: 'submitted' })

    render(<ShowOnce params={params}>{banner}</ShowOnce>)

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('shows nothing when the page is restored with a clean URL (Back or Forward)', () => {
    render(<ShowOnce params={params}>{banner}</ShowOnce>)

    expect(screen.queryByText('Record submitted')).not.toBeInTheDocument()
  })

  it('keeps showing the banner after the parameter has been removed from the URL', () => {
    navigationState.searchParams = new URLSearchParams({ action: 'submitted' })
    const { rerender } = render(<ShowOnce params={params}>{banner}</ShowOnce>)

    navigationState.searchParams = new URLSearchParams()
    rerender(<ShowOnce params={params}>{banner}</ShowOnce>)

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('shows the banner when its parameter appears after the page was mounted without it', () => {
    const { rerender } = render(<ShowOnce params={params}>{banner}</ShowOnce>)
    expect(screen.queryByText('Record submitted')).not.toBeInTheDocument()

    navigationState.searchParams = new URLSearchParams({ action: 'submitted' })
    rerender(<ShowOnce params={params}>{banner}</ShowOnce>)

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('shows the banner when any one of several parameters is in the URL', () => {
    navigationState.searchParams = new URLSearchParams({ userId: '4' })

    render(<ShowOnce params={['action', 'userId']}>{banner}</ShowOnce>)

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('ignores a parameter it does not own', () => {
    window.history.replaceState(null, '', '/records?userId=4')
    navigationState.searchParams = new URLSearchParams({ userId: '4' })

    render(<ShowOnce params={params}>{banner}</ShowOnce>)

    expect(screen.queryByText('Record submitted')).not.toBeInTheDocument()
    expect(window.location.search).toBe('?userId=4')
  })

  it('removes only its own parameters from the address bar', () => {
    window.history.replaceState(null, '', '/records?action=submitted&userId=4&recordType=medicine')
    navigationState.searchParams = new URLSearchParams({ action: 'submitted', userId: '4' })

    render(<ShowOnce params={params}>{banner}</ShowOnce>)

    expect(window.location.pathname).toBe('/records')
    expect(window.location.search).toBe('?userId=4&recordType=medicine')
  })

  it('removes all of its parameters when it has several', () => {
    window.history.replaceState(null, '', '/records?action=invited&userId=4&recordType=medicine')
    navigationState.searchParams = new URLSearchParams({ action: 'invited', userId: '4' })

    render(<ShowOnce params={['action', 'userId']}>{banner}</ShowOnce>)

    expect(window.location.search).toBe('?recordType=medicine')
  })

  it('leaves the address bar alone when none of its parameters are present', () => {
    window.history.replaceState(null, '', '/records?recordType=medicine')
    const replaceState = vi.spyOn(window.history, 'replaceState')

    render(<ShowOnce params={params}>{banner}</ShowOnce>)

    expect(replaceState).not.toHaveBeenCalled()
  })
})
