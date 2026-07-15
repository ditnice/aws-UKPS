'use client'

import { Button } from '@nice-digital/nds-button'
import { Hero } from '@nice-digital/nds-hero'
import { PhaseBanner } from '@nice-digital/nds-phase-banner'

import { Example } from '../../_components/Example'
import { illustration } from '../../_data/illustration'

const heroUrl = '/portal/components/hero'

export function Examples() {
  return (
    <>
      <p>
        Hero always renders a page-level heading, so this catalogue intentionally contains
        additional H1 elements.
      </p>
      <Example title="Light with complete supporting content">
        <Hero
          actions={
            <Button to={heroUrl} variant="cta">
              Browse all guidance
            </Button>
          }
          description="Search recommendations, standards and implementation resources by topic."
          header={
            <PhaseBanner beta>This service is being improved using your feedback.</PhaseBanner>
          }
          image={illustration}
          intro="Evidence-based guidance for health and care"
          title="Find NICE guidance"
        />
      </Example>
      <Example dark title="Dark">
        <Hero
          actions={
            <Button to={heroUrl} variant="inverse">
              Explore quality standards
            </Button>
          }
          description="Identify priority areas for improving care and outcomes."
          intro="Clear, measurable statements"
          isDark
          title="Quality standards"
        />
      </Example>
    </>
  )
}
