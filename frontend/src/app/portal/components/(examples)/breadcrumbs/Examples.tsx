'use client'

import { Breadcrumb, Breadcrumbs } from '@nice-digital/nds-breadcrumbs'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <Example title="Example: breadcrumbs">
      <Breadcrumbs>
        <Breadcrumb to="https://www.nice.org.uk/">Home</Breadcrumb>
        <Breadcrumb to="https://www.nice.org.uk/guidance">NICE guidance</Breadcrumb>
        <Breadcrumb to="/">Published</Breadcrumb>
      </Breadcrumbs>
    </Example>
  )
}
