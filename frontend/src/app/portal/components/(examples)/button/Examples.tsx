'use client'

import { Button } from '@nice-digital/nds-button'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Header default">
        <Button>Do something</Button>
      </Example>

      <Example title="CTA button">
        <Button variant={Button.variants.cta}>CTA button</Button>
      </Example>

      <Example title="Primary button">
        <Button variant={Button.variants.primary}>Primary button</Button>
      </Example>

      <Example title="Secondary button">
        <Button variant={Button.variants.secondary}>Secondary button</Button>
      </Example>

      <Example dark title="Inverse button">
        <Button variant={Button.variants.inverse}>Inverse button</Button>
      </Example>
    </>
  )
}
