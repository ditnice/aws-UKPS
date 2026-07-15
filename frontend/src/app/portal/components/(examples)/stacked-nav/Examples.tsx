'use client'

import { StackedNav, StackedNavLink } from '@nice-digital/nds-stacked-nav'

import { Example } from '../../_components/Example'

const stackedNavPath = '/portal/components/stacked-nav'

export function Examples() {
  return (
    <>
      <Example title="Heading, current item, hint and nested links">
        <StackedNav aria-label="Guidance sections" elementType="h3" label="Guidance sections">
          <StackedNavLink destination={stackedNavPath}>Overview</StackedNavLink>
          <StackedNavLink
            destination={stackedNavPath}
            hint="The section currently being viewed"
            isCurrent
            nested={
              <>
                <StackedNavLink destination={stackedNavPath}>Adults</StackedNavLink>
                <StackedNavLink destination={stackedNavPath}>
                  Children and young people
                </StackedNavLink>
              </>
            }
          >
            Recommendations
          </StackedNavLink>
          <StackedNavLink destination={stackedNavPath}>Evidence</StackedNavLink>
        </StackedNav>
      </Example>

      <Example title="Linked heading">
        <StackedNav
          aria-label="Service resources"
          elementType="h3"
          label="Service resources"
          link={{ destination: stackedNavPath, isCurrent: true }}
        >
          <StackedNavLink destination={stackedNavPath}>Tools and resources</StackedNavLink>
          <StackedNavLink destination={stackedNavPath}>Information for the public</StackedNavLink>
        </StackedNav>
      </Example>
    </>
  )
}
