'use client'

import { Breadcrumb, Breadcrumbs } from '@nice-digital/nds-breadcrumbs'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <Example title="Linked trail and current page">
      <Breadcrumbs>
        <Breadcrumb to="/portal">Portal</Breadcrumb>
        <Breadcrumb to="/portal/components">Components</Breadcrumb>
        <Breadcrumb>Breadcrumbs</Breadcrumb>
      </Breadcrumbs>
    </Example>
  )
}
