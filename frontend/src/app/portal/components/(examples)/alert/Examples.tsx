'use client'

import { Alert } from '@/components/Alert/Alert'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Info alert (announced politely)">
        <Alert type="info">
          <h3>Info alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Success alert (announced politely)">
        <Alert type="success">
          <h3>Success alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Error alert (announced immediately)">
        <Alert type="error">
          <h3>Error alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Caution alert (announced immediately)">
        <Alert type="caution">
          <h3>Caution alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
      <Example title="Info alert (announced immediately)">
        <Alert type="info" nonIntrusive={false}>
          <h3>Info alert</h3>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
        </Alert>
      </Example>
    </>
  )
}
