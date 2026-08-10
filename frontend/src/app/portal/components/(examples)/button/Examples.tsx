'use client'

import { Button } from '@/components/Button/Button'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Header default">
        <Button>Do something</Button>
      </Example>

      <Example title="CTA button">
        <Button variant="cta">CTA button</Button>
      </Example>

      <Example title="Primary button">
        <Button variant="primary">Primary button</Button>
      </Example>

      <Example title="Secondary button">
        <Button variant="secondary">Secondary button</Button>
      </Example>

      <Example dark title="Inverse button">
        <Button variant="inverse">Inverse button</Button>
      </Example>

      <Example title="Link button">
        <Button variant="link">Link button</Button>
      </Example>
    </>
  )
}
