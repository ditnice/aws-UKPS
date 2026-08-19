import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Legal } from './Legal'

afterEach(cleanup)

const legalLinks = [
  { name: 'Accessibility', href: 'https://www.nice.org.uk/accessibility' },
  {
    name: 'Freedom of information',
    href: 'https://www.nice.org.uk/freedom-of-information',
  },
  { name: 'Glossary', href: 'https://www.nice.org.uk/glossary' },
  {
    name: 'Terms and conditions',
    href: 'https://www.nice.org.uk/terms-and-conditions',
  },
  { name: 'Privacy notice', href: 'https://www.nice.org.uk/privacy-notice' },
  { name: 'Cookies', href: 'https://www.nice.org.uk/cookies' },
]

describe('Legal', () => {
  it('renders the legal menu navigation landmark', () => {
    const { asFragment } = render(<Legal />)

    expect(screen.getByRole('navigation', { name: 'Legal menu' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(legalLinks)('renders the $name link', ({ name, href }) => {
    const { asFragment } = render(<Legal />)

    expect(screen.getByRole('link', { name }).getAttribute('href')).toBe(href)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders the copyright notice for the current year', () => {
    const { asFragment, container } = render(<Legal />)

    const year = new Date().getFullYear()
    const copyright = container.querySelector('p')
    expect(copyright?.textContent?.replace(/\s+/g, ' ')).toContain(
      `© NICE ${year}. All rights reserved.`,
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders the notice of rights link', () => {
    const { asFragment } = render(<Legal />)

    expect(screen.getByRole('link', { name: 'Notice of rights' }).getAttribute('href')).toBe(
      'https://www.nice.org.uk/terms-and-conditions#notice-of-rights',
    )
    expect(asFragment()).toMatchSnapshot()
  })
})
