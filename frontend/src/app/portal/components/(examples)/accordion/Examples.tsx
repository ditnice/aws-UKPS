'use client'

import { useState } from 'react'

import { Accordion, AccordionGroup } from '@nice-digital/nds-accordion'

import { Example } from '../../_components/Example'

export function Examples() {
  const [allSectionsOpen, setAllSectionsOpen] = useState(false)

  return (
    <>
      <Example title="Variants and initial state">
        <Accordion title="Eligibility" variant="subtle">
          <p>Check the service criteria before starting an application.</p>
        </Accordion>
        <Accordion open title="Evidence to provide" variant="callout">
          <p>Include a recent assessment and relevant supporting records.</p>
        </Accordion>
        <Accordion title="Important safety information" variant="caution">
          <p>Contact the appropriate clinical service if urgent support is needed.</p>
        </Accordion>
      </Example>

      <Example title="Heading and non-heading titles">
        <Accordion displayTitleAsHeading headingLevel={3} title="A section heading">
          <p>This accordion title is marked up as a level 3 heading.</p>
        </Accordion>
        <Accordion title="A standard title">
          <p>This accordion title uses the default non-heading container.</p>
        </Accordion>
      </Example>

      <Example title="Custom labels">
        <Accordion
          hideLabel="Hide eligibility details"
          showLabel="Show eligibility details"
          title="Eligibility details"
        >
          <p>Applicants must meet the published eligibility criteria.</p>
        </Accordion>
        <Accordion
          hideLabel="Hide contact details"
          open
          showLabel="Show contact details"
          title="Contact details"
        >
          <p>Use the support contact listed for your organisation.</p>
        </Accordion>
      </Example>

      <Example title="Accordion group">
        <AccordionGroup
          onToggle={setAllSectionsOpen}
          toggleText={(isOpen) => (isOpen ? 'Close every topic' : 'Open every topic')}
        >
          <Accordion title="Before you begin">
            <p>Gather the information needed to complete the form.</p>
          </Accordion>
          <Accordion title="After you submit">
            <p>You will receive confirmation that the form has been received.</p>
          </Accordion>
        </AccordionGroup>
        <p aria-live="polite">
          Group callback state: {allSectionsOpen ? 'all topics open' : 'all topics closed'}.
        </p>
      </Example>
    </>
  )
}
