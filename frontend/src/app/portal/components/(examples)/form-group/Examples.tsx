'use client'

import { Checkbox } from '@nice-digital/nds-checkbox'
import { FormGroup } from '@nice-digital/nds-form-group'
import { Input } from '@nice-digital/nds-input'
import { Radio } from '@nice-digital/nds-radio'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <FormGroup legend="Personal information">
          <Input label="First name" name="firstname" />
          <Input label="Surname" name="surname" />
        </FormGroup>
      </Example>
      <Example title="Default form group">
        <FormGroup
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference"
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
      <Example title="Form group with hints">
        <FormGroup
          legend="How would you like us to contact you?"
          hint="We promise not to contact you too often"
        >
          <Checkbox label="Email" value="email" name="contact-preference-hints" />
          <Checkbox label="Telephone" value="phone" name="contact-preference-hints" />
        </FormGroup>
      </Example>
      <Example title="Error messages">
        <FormGroup
          legend="How would you like us to contact you?"
          groupError="Please choose at least one contact method!"
        >
          <Checkbox label="Email" value="email" name="contact-preference-error" />
          <Checkbox label="Telephone" value="phone" name="contact-preference-error" />
        </FormGroup>
      </Example>
      <Example title="Form group with heading level">
        <FormGroup legend="Personal information" headingLevel={3}>
          <Input label="First name" name="firstname-heading-level" />
          <Input label="Surname" name="surname-heading-level" />
        </FormGroup>
      </Example>
    </>
  )
}
