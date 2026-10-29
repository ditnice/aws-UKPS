import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { OrganisationSwitcher } from './OrganisationSwitcher'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

describe('OrganisationSwitcher', () => {
  it('shows the current organisation with a link to change it', () => {
    render(<OrganisationSwitcher organisationName="Example Pharma" />)

    const switcher = screen.getByRole('navigation', { name: 'Organisation switcher' })
    expect(switcher).toHaveTextContent('Example Pharma')
    expect(screen.getByRole('link', { name: 'Change organisation' })).toHaveAttribute(
      'href',
      '/portal/organisations/select',
    )
  })
})
