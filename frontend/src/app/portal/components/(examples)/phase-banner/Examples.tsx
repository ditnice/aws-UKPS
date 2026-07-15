'use client'

import { PhaseBanner } from '@nice-digital/nds-phase-banner'

import { Example } from '../../_components/Example'

const phaseBannerUrl = '/portal/components/phase-banner'

export function Examples() {
  return (
    <>
      <Example title="Alpha">
        <PhaseBanner alpha>
          This is a new service. <a href={phaseBannerUrl}>Send feedback</a> to help us improve it.
        </PhaseBanner>
      </Example>
      <Example title="Beta">
        <PhaseBanner beta>
          This service is being tested. <a href={phaseBannerUrl}>Tell us about your experience</a>.
        </PhaseBanner>
      </Example>
    </>
  )
}
