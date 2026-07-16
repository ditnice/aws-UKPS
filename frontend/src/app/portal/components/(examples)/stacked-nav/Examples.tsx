'use client'

import { StackedNav, StackedNavLink } from '@nice-digital/nds-stacked-nav'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <Example title="Example stacked nav">
      <StackedNav label="Stacked nav label" elementType="h2">
        <StackedNavLink destination="/">Home</StackedNavLink>
        <StackedNavLink
          destination="/about"
          nested={<StackedNavLink destination="/about/test">Nested page</StackedNavLink>}
        >
          About
        </StackedNavLink>
      </StackedNav>
    </Example>
  )
}
