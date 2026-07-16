'use client'

import { Alert } from '@nice-digital/nds-alert'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Info alert">
        <Alert type="info">
          <h3>Info alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Success alert">
        <Alert type="success">
          <h3>Success alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Error alert">
        <Alert type="error">
          <h3>Error alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Caution alert">
        <Alert type="caution">
          <h3>Caution alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Info alert (non-intrusive)">
        <Alert type="info" nonIntrusive>
          <h3>Info alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
    </>
  )
}
