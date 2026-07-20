'use client'

import { HorizontalNav, HorizontalNavLink } from '@nice-digital/nds-horizontal-nav'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <Example title="Horizontal Nav">
      <HorizontalNav aria-label="My navigation">
        <HorizontalNavLink destination="/">Home</HorizontalNavLink>
        <HorizontalNavLink isCurrent destination="/about-us">
          About
        </HorizontalNavLink>
        <HorizontalNavLink destination="/contact">Contact</HorizontalNavLink>
      </HorizontalNav>
    </Example>
  )
}
