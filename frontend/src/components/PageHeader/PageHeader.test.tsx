import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { BackLink } from '@/components/BackLink/BackLink'

import { PageHeader } from './PageHeader'

afterEach(cleanup)

describe('PageHeader', () => {
  it('renders the heading', () => {
    const { asFragment } = render(<PageHeader heading="Sign-in" />)

    expect(screen.getByRole('heading', { name: 'Sign-in' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies the page header wrapper class', () => {
    const { asFragment, container } = render(<PageHeader heading="Sign-in" />)

    const pageHeader = container.querySelector('[data-component="page-header"]')

    expect(pageHeader?.parentElement?.className).toContain('page-header-wrapper')
    expect(asFragment()).toMatchSnapshot()
  })

  it('preserves a supplied className on the wrapper', () => {
    const { asFragment, container } = render(
      <PageHeader className="custom-page-header" heading="Sign-in" />,
    )

    const pageHeader = container.querySelector('[data-component="page-header"]')

    expect(pageHeader?.parentElement?.classList.contains('custom-page-header')).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a back link in the page header navigation slot', () => {
    const { asFragment } = render(
      <PageHeader backLink={<BackLink href="/previous">Back</BackLink>} heading="Sign-in" />,
    )

    expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe('/previous')
    expect(asFragment()).toMatchSnapshot()
  })

  it('prefers backLink over breadcrumbs', () => {
    const { asFragment } = render(
      <PageHeader
        backLink={<BackLink href="/previous">Back</BackLink>}
        breadcrumbs={<nav aria-label="Breadcrumbs">Breadcrumbs</nav>}
        heading="Sign-in"
      />,
    )

    expect(screen.getByRole('link', { name: 'Back' })).toBeDefined()
    expect(screen.queryByLabelText('Breadcrumbs')).toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })
})
