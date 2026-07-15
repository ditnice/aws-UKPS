'use client'

import { Container } from '@nice-digital/nds-container'
import { FullBleed } from '@nice-digital/nds-full-bleed'

import { Example } from '../../_components/Example'
import { illustration } from '../../_data/illustration'

export function Examples() {
  return (
    <>
      <Example fullWidth title="Default teal, small padding">
        <FullBleed padding="small">
          <Container>
            <h3>NICE services</h3>
            <p>Browse guidance, standards and practical implementation resources.</p>
          </Container>
        </FullBleed>
      </Example>
      <Example dark fullWidth title="Dark, small padding">
        <FullBleed padding="small" variant="dark">
          <Container>
            <h3>Clinical guidance</h3>
            <p>Evidence-based recommendations for health and care professionals.</p>
          </Container>
        </FullBleed>
      </Example>
      <Example fullWidth title="Light, medium padding">
        <FullBleed padding="medium" variant="light">
          <Container>
            <h3>Quality standards</h3>
            <p>Priorities for improving the quality of care and services.</p>
          </Container>
        </FullBleed>
      </Example>
      <Example fullWidth title="Transparent, large padding">
        <FullBleed padding="large" variant="transparent">
          <Container>
            <h3>Implementation resources</h3>
            <p>Practical tools to help put recommendations into practice.</p>
          </Container>
        </FullBleed>
      </Example>
      <Example dark fullWidth title="Image with dark treatment">
        <FullBleed backgroundImage={illustration} padding="medium" variant="imageDark">
          <Container>
            <h3>Improving outcomes</h3>
            <p>Use local insight alongside NICE recommendations to plan services.</p>
          </Container>
        </FullBleed>
      </Example>
      <Example fullWidth title="Image with light treatment">
        <FullBleed backgroundImage={illustration} padding="medium" variant="imageLight">
          <Container>
            <h3>Working with communities</h3>
            <p>Make information accessible and involve people in decisions.</p>
          </Container>
        </FullBleed>
      </Example>
    </>
  )
}
