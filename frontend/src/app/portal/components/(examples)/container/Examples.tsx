'use client'

import { Container } from '@nice-digital/nds-container'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Default container">
        <Container>Content here...</Container>
      </Example>
      <Example fullWidth title="Full width container">
        <Container fullWidth>Full width content here...</Container>
      </Example>
    </>
  )
}
