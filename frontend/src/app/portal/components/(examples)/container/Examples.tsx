'use client'

import { Container } from '@nice-digital/nds-container'
import { Panel } from '@nice-digital/nds-panel'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Default constrained container">
        <Container elementType="section" aria-label="Constrained content">
          <Panel>
            <h3>Constrained clinical content</h3>
            <p>This content follows the standard maximum page width.</p>
          </Panel>
        </Container>
      </Example>
      <Example title="Full-width container">
        <Container elementType="section" fullWidth aria-label="Full-width content">
          <Panel variant="supporting">
            <h3>Wider supporting content</h3>
            <p>This container uses 98% of its available parent width.</p>
          </Panel>
        </Container>
      </Example>
    </>
  )
}
