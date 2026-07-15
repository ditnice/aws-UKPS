'use client'

import { Checkbox } from '@nice-digital/nds-checkbox'
import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

export function Examples() {
  return (
    <>
      <Example title="Legend heading, hint and inline children">
        <FormGroup
          headingLevel={3}
          hint="Choose one option."
          inline
          legend="Can we contact you about research?"
          name="showcase-research-contact"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </FormGroup>
      </Example>
      <Example title="Group error">
        <FormGroup
          aria-describedby="showcase-services-error"
          groupError="Select at least one service."
          legend="Which services do you use?"
          name="showcase-services"
        >
          <Checkbox
            aria-invalid="true"
            label="Guidance"
            name="showcase-services"
            value="guidance"
          />
          <Checkbox
            aria-invalid="true"
            label="Standards"
            name="showcase-services"
            value="standards"
          />
        </FormGroup>
        <span className={styles.visuallyHidden} id="showcase-services-error">
          Select at least one service.
        </span>
      </Example>
    </>
  )
}
