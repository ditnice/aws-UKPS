'use client'

import { HorizontalNav, HorizontalNavLink } from '@nice-digital/nds-horizontal-nav'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <Example title="Default links and current item">
      <HorizontalNav aria-label="Navigation component topics">
        <HorizontalNavLink destination="/portal/components/breadcrumbs">
          Breadcrumbs
        </HorizontalNavLink>
        <HorizontalNavLink destination="/portal/components/horizontal-nav" isCurrent>
          Horizontal navigation
        </HorizontalNavLink>
        <HorizontalNavLink destination="/portal/components/in-page-nav">
          In-page navigation
        </HorizontalNavLink>
        <HorizontalNavLink destination="/portal/components/pagination">
          Pagination
        </HorizontalNavLink>
      </HorizontalNav>
    </Example>
  )
}
