'use client'

import { Container } from '@nice-digital/nds-container'
import { FullBleed, fullBleedVariants } from '@nice-digital/nds-full-bleed'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example fullWidth title="Default">
        <FullBleed>
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example fullWidth title="Centre content inside">
        <FullBleed>
          <Container>
            <p>Content here</p>
          </Container>
        </FullBleed>
      </Example>
      <Example fullWidth title="Light">
        <FullBleed variant={fullBleedVariants.light}>
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example dark fullWidth title="Dark">
        <FullBleed variant={fullBleedVariants.dark}>
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example fullWidth title="Transparent">
        <FullBleed variant={fullBleedVariants.transparent}>
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example dark fullWidth title="Dark background image">
        <FullBleed
          variant={fullBleedVariants.imageDark}
          backgroundImage="https://picsum.photos/1000/500"
        >
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example fullWidth title="Light background image">
        <FullBleed
          variant={fullBleedVariants.imageLight}
          backgroundImage="https://picsum.photos/1000/500"
        >
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example fullWidth title="Small padding">
        <FullBleed padding="small">
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example fullWidth title="Medium padding">
        <FullBleed padding="medium">
          <p>Content here</p>
        </FullBleed>
      </Example>
      <Example fullWidth title="Large padding">
        <FullBleed padding="large">
          <p>Content here</p>
        </FullBleed>
      </Example>
    </>
  )
}
