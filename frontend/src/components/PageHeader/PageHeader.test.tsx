import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { BackLink } from '@/components/BackLink/BackLink'

import { PageHeader } from './PageHeader'

describe('PageHeader', () => {
  it('renders the heading', () => {
    const { asFragment } = render(<PageHeader heading="Sign-in" />)

    expect(screen.getByRole('heading', { name: 'Sign-in' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies the page header wrapper class', () => {
    const { container } = render(<PageHeader heading="Sign-in" />)

    const pageHeader = container.querySelector('[data-component="page-header"]')

    expect(pageHeader?.parentElement?.className).toContain('page-header-wrapper')
  })

  it('preserves a supplied className on the wrapper', () => {
    const { container } = render(<PageHeader className="custom-page-header" heading="Sign-in" />)

    const pageHeader = container.querySelector('[data-component="page-header"]')

    expect(pageHeader?.parentElement?.classList.contains('custom-page-header')).toBe(true)
  })

  it('renders a back link in the page header navigation slot', () => {
    const { asFragment } = render(
      <PageHeader backLink={<BackLink href="/previous">Back</BackLink>} heading="Sign-in" />,
    )

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe('/previous')
    expect(asFragment()).toMatchSnapshot()
  })

  it('prefers backLink over breadcrumbs', () => {
    render(
      <PageHeader
        backLink={<BackLink href="/previous">Back</BackLink>}
        breadcrumbs={<nav aria-label="Breadcrumbs">Breadcrumbs</nav>}
        heading="Sign-in"
      />,
    )

    expect(screen.getByRole('link', { name: 'Back' })).toBeInTheDocument()
    expect(screen.queryByLabelText('Breadcrumbs')).toBeNull()
  })
})
