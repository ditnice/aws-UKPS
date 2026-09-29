import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Alert } from './Alert'

afterEach(cleanup)

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

    screen.getByRole('link', { name: 'Got it' })
    expect(screen.queryByRole('link', { name: 'Dismiss' })).toBeNull()
  })
})
