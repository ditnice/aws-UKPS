'use client'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <FormGroup
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-default"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </FormGroup>
      </Example>
      <Example title="Default radios">
        <FormGroup
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-default-example"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </FormGroup>
      </Example>
      <Example title="Inline radios">
        <FormGroup
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-inline"
          inline
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </FormGroup>
      </Example>
      <Example title="Individual radio hint text">
        <FormGroup
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-hint"
        >
          <Radio label="Yes" value="yes" hint="Some helpful hint text" />
          <Radio label="No" value="no" />
        </FormGroup>
      </Example>
      <Example title="Group hint text">
        <FormGroup
          legend="Are you happy for us to contact you in the future?"
          hint="Some helpful hint text"
          name="contact-preference-grouphint"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </FormGroup>
      </Example>
      <Example title="Errors">
        <FormGroup
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-errorhint"
        >
          <Radio label="Yes" value="yes" error="Error message" />
          <Radio label="No" value="no" />
        </FormGroup>
      </Example>
    </>
  )
}
