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

  it('applies the top-only vertical padding class', () => {
    const { asFragment, container } = render(
      <PageHeader heading="Sign-in" verticalPadding="top-only" />,
    )

    expect(container.querySelector('.page-header--vertical-padding-top-only')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('preserves a supplied className with top-only vertical padding', () => {
    const { asFragment, container } = render(
      <PageHeader className="custom-page-header" heading="Sign-in" verticalPadding="top-only" />,
    )

    const pageHeader = container.querySelector('.page-header--vertical-padding-top-only')

    expect(pageHeader?.classList.contains('custom-page-header')).toBe(true)
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
