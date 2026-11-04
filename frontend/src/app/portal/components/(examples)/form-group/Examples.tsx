'use client'

import { Checkbox, Checkboxes } from '@/components/Checkboxes/Checkboxes'
import { FormGroup } from '@/components/FormGroup/FormGroup'
import { Input } from '@/components/Input/Input'
import { Radio, Radios } from '@/components/Radios/Radios'

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
        <Radios
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </Radios>
      </Example>
      <Example title="Inline radios">
        <Radios
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-inline"
          inline
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </Radios>
      </Example>
      <Example title="Form group with hints">
        <Checkboxes
          name="contact-preference-hints"
          legend="How would you like us to contact you?"
          hint="We promise not to contact you too often"
        >
          <Checkbox label="Email" value="email" />
          <Checkbox label="Telephone" value="phone" />
        </Checkboxes>
      </Example>
      <Example title="Error messages">
        <Checkboxes
          name="contact-preference-error"
          legend="How would you like us to contact you?"
          error="Please choose at least one contact method!"
        >
          <Checkbox label="Email" value="email" />
          <Checkbox label="Telephone" value="phone" />
        </Checkboxes>
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
