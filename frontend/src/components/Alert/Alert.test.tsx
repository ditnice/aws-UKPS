import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { navigationState } from '@/test-utils/nextNavigation'

import { Alert } from './Alert'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

describe('Alert', () => {
  it('renders an info alert by default', () => {
    const { asFragment } = render(<Alert>Your changes have been saved.</Alert>)

    const alert = screen.getByText('Your changes have been saved.')
    expect(alert.className).toMatch(/\balert\b/)
    expect(alert.className).toMatch(/\balert--info\b/)
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(['info', 'success'] as const)('announces %s alerts politely', (type) => {
    const { asFragment } = render(<Alert type={type}>Invitation sent</Alert>)

    const alert = screen.getByText('Invitation sent')
    expect(alert.getAttribute('aria-live')).toBe('polite')
    expect(alert.getAttribute('aria-atomic')).toBe('true')
    expect(alert.getAttribute('role')).toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(['caution', 'error'] as const)('announces %s alerts assertively', (type) => {
    const { asFragment } = render(<Alert type={type}>Something went wrong</Alert>)

    const alert = screen.getByRole('alert')
    expect(alert.textContent).toBe('Something went wrong')
    expect(alert.getAttribute('aria-live')).toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('lets the announcement behaviour be overridden per alert', () => {
    render(
      <Alert type="error" nonIntrusive>
        Two of the six files could not be uploaded
      </Alert>,
    )

    const alert = screen.getByText('Two of the six files could not be uploaded')
    expect(alert.getAttribute('aria-live')).toBe('polite')
    expect(alert.getAttribute('role')).toBeNull()
  })

  it('passes HTML attributes through to the alert', () => {
    render(
      <Alert type="caution" data-testid="deadline-alert">
        The deadline has passed
      </Alert>,
    )

    expect(screen.getByTestId('deadline-alert').className).toMatch(/\balert--caution\b/)
  })
})

describe('Alert showOnce', () => {
  it('shows the alert when a banner parameter is in the URL', () => {
    navigationState.searchParams = new URLSearchParams({ action: 'submitted' })

    render(
      <Alert type="success" showOnce="action">
        Record submitted
      </Alert>,
    )

    expect(screen.getByText('Record submitted')).toBeInTheDocument()
  })

  it('accepts several parameters', () => {
    navigationState.searchParams = new URLSearchParams({ userId: '4' })

    render(
      <Alert type="success" showOnce={['action', 'userId']}>
        Invitation sent
      </Alert>,
    )

    expect(screen.getByText('Invitation sent')).toBeInTheDocument()
  })

  it('renders nothing when none of its parameters are in the URL', () => {
    navigationState.searchParams = new URLSearchParams({ userId: '4' })

    render(
      <Alert type="success" showOnce="action">
        Record submitted
      </Alert>,
    )

    expect(screen.queryByText('Record submitted')).not.toBeInTheDocument()
  })

  it('always shows the alert without showOnce', () => {
    render(<Alert type="success">Password changed</Alert>)

    expect(screen.getByText('Password changed')).toBeInTheDocument()
  })
})

describe('Alert dismiss button', () => {
  it('renders no dismiss button when no dismissHref is given', () => {
    render(<Alert type="success">Invitation sent</Alert>)

    expect(screen.queryByRole('link')).toBeNull()
  })

  it('renders a dismiss link pointing at the given href', () => {
    const { asFragment } = render(
      <Alert type="success" dismissHref="/portal/organisations/2">
        User removed from organisation
      </Alert>,
    )

    const dismiss = screen.getByRole('link', { name: 'Dismiss' })
    expect(dismiss.getAttribute('href')).toBe('/portal/organisations/2')
    expect(asFragment()).toMatchSnapshot()
  })

  it('lets the dismiss link text be overridden', () => {
    render(
      <Alert type="success" dismissHref="/portal/organisations/2" dismissText="Got it">
        User removed from organisation
      </Alert>,
    )

    expect(screen.getByRole('link', { name: 'Got it' })).not.toBeNull()
    expect(screen.queryByRole('link', { name: 'Dismiss' })).toBeNull()
  })
})
