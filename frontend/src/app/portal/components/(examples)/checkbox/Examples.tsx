'use client'

import { Checkbox } from '@nice-digital/nds-checkbox'
import { FormGroup } from '@nice-digital/nds-form-group'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <FormGroup
          legend="How would you like us to contact you?"
          hint="Please select all that apply."
        >
          <Checkbox name="contact-example" value="email" label="Email" />
          <Checkbox name="contact-example" value="telephone" label="Telephone" />
          <Checkbox name="contact-example" value="sms" label="Text message" />
        </FormGroup>
      </Example>
      <Example title="Single checkboxes">
        <Checkbox
          name="terms-and-conditions"
          value="agree"
          label="I agree to the terms and conditions"
        />
      </Example>
      <Example title="Grouped checkboxes">
        <FormGroup
          legend="How would you like us to contact you?"
          hint="Please select all that apply."
        >
          <Checkbox name="contact-grouped" value="email" label="Email" />
          <Checkbox name="contact-grouped" value="telephone" label="Telephone" />
          <Checkbox name="contact-grouped" value="sms" label="Text message" />
        </FormGroup>
      </Example>
      <Example title='Add an option for "none"'>
        <FormGroup legend="How would you like us to contact you?">
          <Checkbox name="contact-grouped-none" value="email" label="Email" />
          <Checkbox name="contact-grouped-none" value="telephone" label="Telephone" />
          <Checkbox name="contact-grouped-none" value="sms" label="Text message" />
          <Checkbox
            name="contact-grouped-none"
            value="none"
            label="I do not wish to be contacted"
          />
        </FormGroup>
      </Example>
      <Example title="Inline checkboxes">
        <FormGroup inline legend="How would you like us to contact you?">
          <Checkbox name="contact-grouped-inline" value="email" label="Email" />
          <Checkbox name="contact-grouped-inline" value="telephone" label="Telephone" />
          <Checkbox name="contact-grouped-inline" value="sms" label="Text message" />
        </FormGroup>
      </Example>
      <Example title="Individual checkbox hint text">
        <Checkbox
          name="newsletter-subscribe"
          value="agree"
          label="Yes, sign me up to the newsletter"
          hint="You can unsubscribe at any time."
        />
      </Example>
      <Example title="Grouped checkboxes hint text">
        <FormGroup
          legend="How would you like us to contact you?"
          hint="Please select all that apply."
        >
          <Checkbox name="contact-grouped-hint" value="email" label="Email" />
          <Checkbox name="contact-grouped-hint" value="telephone" label="Telephone" />
          <Checkbox name="contact-grouped-hint" value="sms" label="Text message" />
        </FormGroup>
      </Example>
      <Example title="Errors">
        <Checkbox
          name="terms-and-conditions-error-demo"
          value="agree"
          label="I agree to the terms and conditions"
          error="You must agree to the terms and conditions."
        />
      </Example>
    </>
  )
}
