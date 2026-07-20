'use client'

import { PhaseBanner } from '@nice-digital/nds-phase-banner'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Alpha">
        <PhaseBanner alpha>
          This is a new service - your <a href="#">feedback</a> will help us to improve it.
        </PhaseBanner>
      </Example>
      <Example title="Beta">
        <PhaseBanner beta>
          This is a new service - your <a href="#">feedback</a> will help us to improve it.
        </PhaseBanner>
      </Example>
    </>
  )
}
