'use client'

import { Panel } from '@nice-digital/nds-panel'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Default">
        <Panel>
          <h2>Panel heading</h2>
        </Panel>
      </Example>
      <Example title="Primary">
        <Panel variant="primary">
          <h2>Panel heading</h2>
        </Panel>
      </Example>
      <Example title="Impact">
        <Panel variant="impact">
          <h2>Panel heading</h2>
        </Panel>
      </Example>
    </>
  )
}
