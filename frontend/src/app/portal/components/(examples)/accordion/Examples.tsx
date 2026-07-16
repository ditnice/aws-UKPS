'use client'

import { Accordion, AccordionGroup } from '@nice-digital/nds-accordion'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Header default">
        <Accordion title="Accordion title subtle variant (default)">
          Our monthly newsletter, keeping you up to date with{' '}
          <a href="#">important developments at NICE</a>
        </Accordion>
      </Example>

      <Example title="Default accordion">
        <Accordion title="Default accordion - subtle variant (default variant)">
          <p>
            Our monthly newsletter, keeping you up to date with
            <a href="#">important developments at NICE</a>
          </p>
        </Accordion>
      </Example>

      <Example title="Callout accordion">
        <Accordion title="Callout accordion title" variant="callout">
          <p>Callout accordion content</p>
        </Accordion>
      </Example>

      <Example title="Caution accordion">
        <Accordion title="Caution accordion title" variant="caution">
          <p>Caution variant content</p>
        </Accordion>
      </Example>

      <Example title="Accordion with heading">
        <Accordion
          title="Accordion with heading title"
          displayTitleAsHeading={true}
          headingLevel={2}
        >
          <p>Inner content of accordion</p>
        </Accordion>
      </Example>

      <Example title="Accordion group">
        <AccordionGroup>
          <Accordion key="1" title="Accordion 1">
            <p>Accordion 1 body</p>
          </Accordion>
          <Accordion key="2" title="Accordion 2">
            <p>Accordion 2 body</p>
          </Accordion>
        </AccordionGroup>
      </Example>
    </>
  )
}
